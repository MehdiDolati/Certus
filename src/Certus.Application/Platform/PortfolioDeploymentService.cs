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

        // Detect if path uses FILE_COMMON (Common\Files\) or is per-terminal.
        // Normalize to backslashes only for the string check — pass the original
        // path (with native separators) to the derivation methods so filesystem
        // operations work on both Windows and Linux (CI).
        var normalizedPath = filePath.Replace('/', '\\');
        bool isCommonPath = normalizedPath.Contains(@"Common\Files\", StringComparison.OrdinalIgnoreCase);

        if (isCommonPath)
            return await DeriveFromCommonPathAsync(filePath);
        else
            return await DeriveFromTerminalPathAsync(filePath);
    }

    private static Task<DeriveMT4PathResult> DeriveFromTerminalPathAsync(string terminalPath)
    {
        string mt4DataPath;
        if (File.Exists(terminalPath))
        {
            var certusDir = Path.GetDirectoryName(terminalPath)!;
            mt4DataPath = Path.GetDirectoryName(certusDir) ?? terminalPath;
        }
        else if (Directory.Exists(terminalPath))
        {
            mt4DataPath = Path.GetDirectoryName(terminalPath) ?? terminalPath;
        }
        else
        {
            return Task.FromResult(new DeriveMT4PathResult
            {
                Success = false,
                ErrorMessage = "Cannot determine MT4 data path from connection file path"
            });
        }

        var expertsPath = Path.Combine(mt4DataPath, "MQL4", "Experts");

        return Task.FromResult(new DeriveMT4PathResult
        {
            Success = true,
            IsCommonPath = false,
            MT4DataPath = mt4DataPath,
            ExpertsPath = expertsPath
        });
    }

    private static Task<DeriveMT4PathResult> DeriveFromCommonPathAsync(string commonPath)
    {
        // commonPath is like: C:\...\MetaQuotes\Common\Files\Certus\portfolio_status.json
        // or: C:\...\MetaQuotes\Common\Files\Certus\
        //
        // Step 1: Navigate up to find MetaQuotes root
        string commonFilesDir;
        if (File.Exists(commonPath))
        {
            var certusDir = Path.GetDirectoryName(commonPath)!;
            commonFilesDir = Path.GetDirectoryName(certusDir)!; // Common\Files
        }
        else if (Directory.Exists(commonPath))
        {
            var dirName = Path.GetFileName(commonPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            if (dirName.Equals("Files", StringComparison.OrdinalIgnoreCase))
            {
                commonFilesDir = commonPath;
            }
            else
            {
                commonFilesDir = Path.GetDirectoryName(commonPath)!; // Common\Files
            }
        }
        else
        {
            // Path doesn't exist on disk — try to derive from the path string itself
            // Handles case where connection was created but EA hasn't written files yet
            var certusDir = Path.GetDirectoryName(commonPath);
            if (certusDir != null)
            {
                var dirName = Path.GetFileName(certusDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                if (dirName.Equals("Files", StringComparison.OrdinalIgnoreCase))
                {
                    commonFilesDir = certusDir;
                }
                else
                {
                    commonFilesDir = Path.GetDirectoryName(certusDir)!;
                }
            }
            else
            {
                return Task.FromResult(new DeriveMT4PathResult
                {
                    Success = false,
                    IsCommonPath = true,
                    ErrorMessage = "Cannot determine Common path from connection file path"
                });
            }
        }

        // Common\Files -> Common -> MetaQuotes
        var commonDir = Path.GetDirectoryName(commonFilesDir)!;
        var metaQuotesRoot = Path.GetDirectoryName(commonDir)!;

        // Build Common/Files/Certus/ path for activation JSON
        var certusCommonPath = commonPath.EndsWith("portfolio_status.json", StringComparison.OrdinalIgnoreCase)
            ? Path.GetDirectoryName(commonPath)!
            : commonPath;

        // Step 2: Scan MetaQuotes\Terminal\ for hash subdirs with MQL4\Experts
        var terminalRoot = Path.Combine(metaQuotesRoot, "Terminal");
        if (!Directory.Exists(terminalRoot))
        {
            return Task.FromResult(new DeriveMT4PathResult
            {
                Success = false,
                IsCommonPath = true,
                CommonFilesPath = certusCommonPath,
                ErrorMessage = $"No Terminal directory found at {terminalRoot}"
            });
        }

        var terminalDirs = Directory.GetDirectories(terminalRoot);
        var candidates = new List<TerminalCandidate>();

        foreach (var terminalDir in terminalDirs)
        {
            var dirName = Path.GetFileName(terminalDir);
            // Skip the "Common" directory itself
            if (dirName.Equals("Common", StringComparison.OrdinalIgnoreCase))
                continue;

            var expertsPath = Path.Combine(terminalDir, "MQL4", "Experts");
            if (Directory.Exists(expertsPath))
            {
                candidates.Add(new TerminalCandidate
                {
                    MT4DataPath = terminalDir,
                    ExpertsPath = expertsPath,
                    DisplayName = dirName
                });
            }
        }

        if (candidates.Count == 0)
        {
            return Task.FromResult(new DeriveMT4PathResult
            {
                Success = false,
                IsCommonPath = true,
                CommonFilesPath = certusCommonPath,
                ErrorMessage = "No terminal found with MQL4/Experts directory"
            });
        }

        if (candidates.Count == 1)
        {
            return Task.FromResult(new DeriveMT4PathResult
            {
                Success = true,
                IsCommonPath = true,
                CommonFilesPath = certusCommonPath,
                MT4DataPath = candidates[0].MT4DataPath,
                ExpertsPath = candidates[0].ExpertsPath,
                TerminalCandidates = candidates
            });
        }

        // Multiple candidates — return all for UI selection
        return Task.FromResult(new DeriveMT4PathResult
        {
            Success = false,
            IsCommonPath = true,
            CommonFilesPath = certusCommonPath,
            TerminalCandidates = candidates,
            ErrorMessage = $"Multiple terminals found ({candidates.Count}). Please select one."
        });
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
            else if (!string.IsNullOrWhiteSpace(request.SelectedTerminalPath))
            {
                targetExpertsPath = Path.Combine(request.SelectedTerminalPath, "MQL4", "Experts");
            }
            else
            {
                var pathResult = await DeriveMT4PathAsync(request.ConnectionId);
                if (!pathResult.Success && pathResult.TerminalCandidates.Count == 0)
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

        // Use CommonFilesPath when available (FILE_COMMON), fallback to per-terminal
        var certusFilesPath = !string.IsNullOrEmpty(pathResult.CommonFilesPath)
            ? pathResult.CommonFilesPath
            : Path.Combine(mt4DataPath, "Files", "Certus");

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
