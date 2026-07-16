using Certus.Application.Platform.DTOs;
using Certus.Domain.Platform.Aggregates;
using Certus.Domain.Platform.Enums;
using Certus.Domain.Platform.Interfaces;
using Certus.Domain.Platform.Repositories;
using Certus.Domain.RiskAndPortfolio.Aggregates;
using Certus.Domain.RiskAndPortfolio.Enums;
using Certus.Domain.RiskAndPortfolio.Repositories;
using Certus.Domain.Strategy.Aggregates;
using Certus.Domain.Strategy.Enums;
using Certus.Domain.Strategy.Repositories;
using Certus.Domain.Strategy.ValueObjects;
using Certus.Domain.SharedKernel;

namespace Certus.Application.Platform;

public class PortfolioDeploymentService : IPortfolioDeploymentService
{
    private readonly IPortfolioDeploymentRepository _deploymentRepo;
    private readonly IPlatformConnectionRepository _connectionRepo;
    private readonly IPortfolioRepository _portfolioRepo;
    private readonly IStrategyDefinitionRepository _strategyRepo;
    private readonly IPlatformPluginLoader _pluginLoader;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public PortfolioDeploymentService(
        IPortfolioDeploymentRepository deploymentRepo,
        IPlatformConnectionRepository connectionRepo,
        IPortfolioRepository portfolioRepo,
        IStrategyDefinitionRepository strategyRepo,
        IPlatformPluginLoader pluginLoader,
        IDomainEventDispatcher eventDispatcher)
    {
        _deploymentRepo = deploymentRepo;
        _connectionRepo = connectionRepo;
        _portfolioRepo = portfolioRepo;
        _strategyRepo = strategyRepo;
        _pluginLoader = pluginLoader;
        _eventDispatcher = eventDispatcher;
    }

    public Task<ValidateFolderResult> ValidateFolderAsync(string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
        {
            return Task.FromResult(new ValidateFolderResult
            {
                IsValid = false,
                ErrorMessage = "Folder does not exist or is invalid"
            });
        }

        var ex4Files = Directory.GetFiles(folderPath, "*.ex4", SearchOption.TopDirectoryOnly);

        if (ex4Files.Length == 0)
        {
            return Task.FromResult(new ValidateFolderResult
            {
                IsValid = false,
                ErrorMessage = "No .ex4 files found in root folder"
            });
        }

        var eaNames = ex4Files
            .Select(f => Path.GetFileNameWithoutExtension(f))
            .OrderBy(n => n)
            .ToList();

        return Task.FromResult(new ValidateFolderResult
        {
            IsValid = true,
            EANames = eaNames,
            EACount = ex4Files.Length
        });
    }

    public async Task<List<PlatformConnectionDto>> GetAvailableConnectionsAsync()
    {
        var connections = await _connectionRepo.GetAllAsync();
        return connections.Select(c => new PlatformConnectionDto
        {
            Id = c.Id,
            PlatformId = c.PlatformId,
            PlatformName = c.PlatformName,
            Status = Domain.Platform.Enums.PlatformConnectionStatus.Connected
        }).ToList();
    }

    public async Task<DeriveMT4PathResult> DeriveMT4PathAsync(Guid connectionId)
    {
        var connection = await _connectionRepo.GetByIdAsync(connectionId);
        if (connection == null)
        {
            return new DeriveMT4PathResult
            {
                Success = false,
                ErrorMessage = "Connection not found"
            };
        }

        var filePath = connection.Config.FilePath;
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return new DeriveMT4PathResult
            {
                Success = false,
                ErrorMessage = "Connection has no file path configured"
            };
        }

        // Derive MT4 data folder from connection file path
        // Example: C:\Users\Mehdi\AppData\Roaming\MetaQuotes\Terminal\ABC123\Certus\portfolio_status.json
        // Should derive: C:\Users\Mehdi\AppData\Roaming\MetaQuotes\Terminal\ABC123
        string mt4DataPath;
        if (File.Exists(filePath))
        {
            // filePath is a file - go up to find the terminal hash directory
            var certusDir = Path.GetDirectoryName(filePath);
            mt4DataPath = Path.GetDirectoryName(certusDir) ?? filePath;
        }
        else if (Directory.Exists(filePath))
        {
            // filePath is a directory (Certus folder) - go up one level
            mt4DataPath = Path.GetDirectoryName(filePath) ?? filePath;
        }
        else
        {
            return new DeriveMT4PathResult
            {
                Success = false,
                ErrorMessage = "Cannot determine MT4 data path from connection file path"
            };
        }

        var expertsPath = Path.Combine(mt4DataPath, "MQL4", "Experts");

        return new DeriveMT4PathResult
        {
            Success = true,
            MT4DataPath = mt4DataPath,
            ExpertsPath = expertsPath
        };
    }

    public Task<CheckConflictsResult> CheckConflictsAsync(string sourceFolder, string targetExpertsPath)
    {
        var sourceFiles = Directory.GetFiles(sourceFolder, "*.ex4", SearchOption.TopDirectoryOnly);
        var totalFiles = sourceFiles.Length;

        var conflicts = new List<string>();
        if (Directory.Exists(targetExpertsPath))
        {
            var targetFiles = Directory.GetFiles(targetExpertsPath, "*.ex4", SearchOption.TopDirectoryOnly)
                .Select(f => Path.GetFileName(f))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            conflicts = sourceFiles
                .Select(f => Path.GetFileName(f))
                .Where(f => targetFiles.Contains(f))
                .ToList();
        }

        return Task.FromResult(new CheckConflictsResult
        {
            ConflictingFiles = conflicts,
            TotalFiles = totalFiles,
            NewFiles = totalFiles - conflicts.Count
        });
    }

    public async Task<DeployPortfolioResult> DeployAsync(DeployPortfolioRequest request)
    {
        try
        {
            // Validate source folder
            var validation = await ValidateFolderAsync(request.SourceFolderPath);
            if (!validation.IsValid)
            {
                return new DeployPortfolioResult
                {
                    Success = false,
                    ErrorMessage = validation.ErrorMessage
                };
            }

            // Get connection and derive target path
            var connection = await _connectionRepo.GetByIdAsync(request.ConnectionId);
            if (connection == null)
            {
                return new DeployPortfolioResult
                {
                    Success = false,
                    ErrorMessage = "Connection not found"
                };
            }

            string targetExpertsPath;
            if (!string.IsNullOrWhiteSpace(request.ManualMT4Path))
            {
                targetExpertsPath = Path.Combine(request.ManualMT4Path, "MQL4", "Experts");
            }
            else
            {
                var pathResult = await DeriveMT4PathAsync(request.ConnectionId);
                if (!pathResult.Success)
                {
                    return new DeployPortfolioResult
                    {
                        Success = false,
                        ErrorMessage = pathResult.ErrorMessage
                    };
                }
                targetExpertsPath = pathResult.ExpertsPath!;
            }

            // Create Portfolio entity
            var portfolioName = request.PortfolioName
                ?? Path.GetFileName(request.SourceFolderPath);

            var portfolio = new Portfolio(
                Guid.NewGuid(),
                portfolioName,
                0,
                0,
                Money.Zero(Currency.USD),
                $"Deployed from {request.SourceFolderPath}");

            portfolio.SetPlatformReference(request.ConnectionId, portfolioName);
            await _portfolioRepo.AddAsync(portfolio);

            // Create StrategyDefinition entities for each EA
            var ex4Files = Directory.GetFiles(request.SourceFolderPath, "*.ex4", SearchOption.TopDirectoryOnly);
            var strategies = new List<StrategyDefinition>();

            foreach (var file in ex4Files)
            {
                var eaName = Path.GetFileNameWithoutExtension(file);
                var strategy = new StrategyDefinition(
                    Guid.NewGuid(),
                    eaName,
                    new StrategyType(StrategyCategory.Custom, "ExpertAdvisor"),
                    0,
                    $"MT4 Expert Advisor: {eaName}");

                await _strategyRepo.AddAsync(strategy);
                strategy.AssignToPortfolio(portfolio.Id, new Weight(1.0m / ex4Files.Length));
                strategies.Add(strategy);
            }

            // Create deployment entity
            var deployment = PortfolioDeployment.Create(
                portfolio.Id,
                request.ConnectionId,
                request.SourceFolderPath,
                targetExpertsPath,
                validation.EACount);

            await _deploymentRepo.AddAsync(deployment);
            deployment.StartCopying();

            // Copy files
            int filesCopied = 0;
            int filesSkipped = 0;

            foreach (var file in ex4Files)
            {
                var fileName = Path.GetFileName(file);
                var targetPath = Path.Combine(targetExpertsPath, fileName);

                var shouldOverwrite = request.ConflictResolutions?.TryGetValue(fileName, out var action) == true
                    ? action == ConflictAction.Overwrite
                    : true; // Default: overwrite

                if (File.Exists(targetPath) && !shouldOverwrite)
                {
                    filesSkipped++;
                    continue;
                }

                Directory.CreateDirectory(targetExpertsPath);
                await CopyFileWithRetryAsync(file, targetPath);
                filesCopied++;
            }

            deployment.MarkDeployed();

            // Try to auto-activate EAs
            bool activationPending = false;
            try
            {
                await ActivateEAsAsync(deployment, connection, ex4Files);
            }
            catch
            {
                activationPending = true;
            }

            // Start monitoring
            deployment.StartMonitoring();

            await _deploymentRepo.SaveChangesAsync();

            return new DeployPortfolioResult
            {
                Success = true,
                PortfolioId = portfolio.Id,
                DeploymentId = deployment.Id,
                FilesCopied = filesCopied,
                FilesSkipped = filesSkipped,
                ActivationPending = activationPending
            };
        }
        catch (Exception ex)
        {
            return new DeployPortfolioResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<PortfolioDeploymentDto?> GetDeploymentStatusAsync(Guid deploymentId)
    {
        var deployment = await _deploymentRepo.GetByIdAsync(deploymentId);
        if (deployment == null) return null;

        return await MapToDtoAsync(deployment);
    }

    public async Task StopMonitoringAsync(Guid deploymentId)
    {
        var deployment = await _deploymentRepo.GetByIdAsync(deploymentId)
            ?? throw new InvalidOperationException($"Deployment not found: {deploymentId}");

        deployment.Stop();
        await _deploymentRepo.SaveChangesAsync();
    }

    public async Task<List<PortfolioDeploymentDto>> GetDeploymentsAsync()
    {
        var deployments = await _deploymentRepo.GetAllAsync();
        var result = new List<PortfolioDeploymentDto>();

        foreach (var d in deployments)
        {
            result.Add(await MapToDtoAsync(d));
        }

        return result;
    }

    private async Task ActivateEAsAsync(
        PortfolioDeployment deployment,
        PlatformConnection connection,
        string[] ex4Files)
    {
        // Derive MT4 data path
        var pathResult = await DeriveMT4PathAsync(connection.Id);
        if (!pathResult.Success || string.IsNullOrEmpty(pathResult.MT4DataPath))
            throw new InvalidOperationException("Cannot determine MT4 data path for activation");

        var mt4DataPath = pathResult.MT4DataPath;
        var certusFilesPath = Path.Combine(mt4DataPath, "Files", "Certus");
        var scriptsPath = Path.Combine(mt4DataPath, "MQL4", "Scripts");

        Directory.CreateDirectory(certusFilesPath);
        Directory.CreateDirectory(scriptsPath);

        // Generate activation config
        var eaList = ex4Files.Select(f => new
        {
            name = Path.GetFileNameWithoutExtension(f),
            symbol = "EURUSD",
            timeframe = "H1",
            parameters = new { }
        }).ToList();

        var config = System.Text.Json.JsonSerializer.Serialize(new { eas = eaList },
            new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

        await File.WriteAllTextAsync(
            Path.Combine(certusFilesPath, "certus_activation.json"),
            config);
    }

    private static async Task CopyFileWithRetryAsync(string source, string destination, int maxRetries = 3)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                File.Copy(source, destination, overwrite: true);
                return;
            }
            catch (IOException) when (i < maxRetries - 1)
            {
                await Task.Delay(100 * (int)Math.Pow(2, i)); // Exponential backoff
            }
        }
    }

    private async Task<PortfolioDeploymentDto> MapToDtoAsync(PortfolioDeployment deployment)
    {
        var portfolio = await _portfolioRepo.GetByIdAsync(deployment.PortfolioId);
        var connection = await _connectionRepo.GetByIdAsync(deployment.ConnectionId);

        return new PortfolioDeploymentDto
        {
            Id = deployment.Id,
            PortfolioId = deployment.PortfolioId,
            PortfolioName = portfolio?.Name ?? "Unknown",
            ConnectionId = deployment.ConnectionId,
            ConnectionName = connection?.PlatformName ?? "Unknown",
            SourceFolderPath = deployment.SourceFolderPath,
            Status = deployment.Status,
            EACount = deployment.EACount,
            DeployedAt = deployment.DeployedAt,
            MonitoringStartedAt = deployment.MonitoringStartedAt,
            ErrorMessage = deployment.ErrorMessage
        };
    }
}
