using Certus.Application.Platform.DTOs;
using Certus.Domain.Platform.Aggregates;
using Certus.Domain.Platform.Enums;
using Certus.Domain.Platform.Events;
using Certus.Domain.Platform.Interfaces;
using Certus.Domain.Platform.Repositories;
using Certus.Domain.Platform.ValueObjects;
using Certus.Domain.RiskAndPortfolio.Aggregates;
using Certus.Domain.RiskAndPortfolio.Repositories;
using Certus.Domain.SharedKernel;

namespace Certus.Application.Platform;

public class PlatformService : IPlatformService
{
    private readonly IPlatformPluginLoader _pluginLoader;
    private readonly IPlatformConnectionRepository _connectionRepo;
    private readonly IPlatformDataRepository _dataRepo;
    private readonly IPortfolioRepository _portfolioRepo;
    private readonly IImportedTradeRepository _tradeRepo;
    private readonly IFileImportService _fileImportService;
    private readonly IConnectionStatusStore _statusStore;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public PlatformService(
        IPlatformPluginLoader pluginLoader,
        IPlatformConnectionRepository connectionRepo,
        IPlatformDataRepository dataRepo,
        IPortfolioRepository portfolioRepo,
        IImportedTradeRepository tradeRepo,
        IFileImportService fileImportService,
        IConnectionStatusStore statusStore,
        IDomainEventDispatcher eventDispatcher)
    {
        _pluginLoader = pluginLoader;
        _connectionRepo = connectionRepo;
        _dataRepo = dataRepo;
        _portfolioRepo = portfolioRepo;
        _tradeRepo = tradeRepo;
        _fileImportService = fileImportService;
        _statusStore = statusStore;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<PlatformConnectionDto> ConnectAsync(ConnectPlatformRequest request)
    {
        var plugin = _pluginLoader.GetPluginForPlatform(request.PlatformId)
            ?? throw new InvalidOperationException($"No plugin found for platform: {request.PlatformId}");

        // Check for existing connection with same platform and path
        if (!string.IsNullOrEmpty(request.FilePath))
        {
            var existing = await _connectionRepo.GetByPlatformAndPathAsync(request.PlatformId, request.FilePath);
            var match = existing.FirstOrDefault();
            if (match != null)
            {
                var currentStatus = _statusStore.Get(match.Id);
                if (currentStatus.State != PlatformConnectionStatus.Connected)
                {
                    var adapter = plugin.CreateAdapter(match.Config);
                    var result = await adapter.ConnectAsync(match.Config);
                    if (result.Success)
                    {
                        _statusStore.Set(match.Id, currentStatus.WithConnected());
                    }
                }
                return MapToDto(match, _statusStore.Get(match.Id));
            }
        }

        var config = new PlatformConfig
        {
            PlatformType = Enum.Parse<PlatformType>(request.PlatformId, true),
            DataFormat = DataFormat.Json,
            FilePath = request.FilePath,
            ServerAddress = request.ServerAddress,
            Port = request.Port,
            ApiKey = request.ApiKey,
            UseFileWatcher = request.UseFileWatcher
        };

        var adapterForNew = plugin.CreateAdapter(config);
        var connectResult = await adapterForNew.ConnectAsync(config);

        if (!connectResult.Success)
            throw new InvalidOperationException($"Failed to connect: {connectResult.ErrorMessage}");

        var connection = PlatformConnection.Create(
            request.PlatformId,
            plugin.PluginName,
            config);

        await _connectionRepo.AddAsync(connection);
        _statusStore.Set(connection.Id, ConnectionStatus.Disconnected.WithConnected());

        if (!string.IsNullOrEmpty(request.FilePath) && request.UseFileWatcher)
        {
            _fileImportService.StartWatching(connection.Id, request.FilePath);
            _fileImportService.FileChanged += async (sender, args) =>
            {
                if (args.ConnectionId == connection.Id)
                {
                    _statusStore.Set(connection.Id, _statusStore.Get(connection.Id).WithLastDataReceived());
                    await AutoImportFromChangeAsync(connection, args);
                }
            };
        }

        await _eventDispatcher.DispatchAsync(new PlatformConnected
        {
            ConnectionId = connection.Id,
            PlatformId = request.PlatformId
        });

        return MapToDto(connection, _statusStore.Get(connection.Id));
    }

    public async Task DisconnectAsync(Guid connectionId)
    {
        var connection = await _connectionRepo.GetByIdAsync(connectionId)
            ?? throw new InvalidOperationException($"Connection not found: {connectionId}");

        _fileImportService.StopWatching(connectionId);
        _statusStore.Set(connectionId, _statusStore.Get(connectionId).WithDisconnected("User disconnected"));
    }

    public async Task<List<PlatformConnectionDto>> GetConnectionsAsync()
    {
        var connections = await _connectionRepo.GetAllAsync();
        foreach (var c in connections)
            CheckStaleness(c);
        return connections.Select(c => MapToDto(c, _statusStore.Get(c.Id))).ToList();
    }

    public async Task<PlatformConnectionDto?> GetConnectionAsync(Guid connectionId)
    {
        var connection = await _connectionRepo.GetByIdAsync(connectionId);
        if (connection == null) return null;
        CheckStaleness(connection);
        return MapToDto(connection, _statusStore.Get(connectionId));
    }

    public async Task<PlatformStatusDto> GetStatusAsync(Guid connectionId)
    {
        var connection = await _connectionRepo.GetByIdAsync(connectionId)
            ?? throw new InvalidOperationException($"Connection not found: {connectionId}");

        var status = _statusStore.Get(connectionId);

        return new PlatformStatusDto
        {
            ConnectionId = connection.Id,
            PlatformName = connection.PlatformName,
            Status = status.State,
            IsConnected = status.State == PlatformConnectionStatus.Connected,
            ConnectedAt = status.ConnectedAt,
            LastDataReceivedAt = status.LastDataReceivedAt,
            ErrorMessage = status.ErrorMessage
        };
    }

    public async Task<PlatformPortfolioDto?> ImportPortfolioAsync(Guid connectionId, string externalPortfolioId, string? portfolioName = null)
    {
        var connection = await _connectionRepo.GetByIdAsync(connectionId)
            ?? throw new InvalidOperationException($"Connection not found: {connectionId}");

        var platformPortfolio = await _dataRepo.GetLatestPortfolioSnapshotAsync(connectionId, externalPortfolioId);
        if (platformPortfolio == null)
            return null;

        var portfolio = new Portfolio(
            Guid.NewGuid(),
            portfolioName ?? platformPortfolio.Name,
            0,
            0,
            new Money(platformPortfolio.Balance, Currency.USD),
            $"Imported from {connection.PlatformName}");

        portfolio.SetPlatformReference(connectionId, externalPortfolioId);

        await _portfolioRepo.AddAsync(portfolio);

        await _eventDispatcher.DispatchAsync(new PortfolioImported
        {
            PortfolioId = portfolio.Id,
            ConnectionId = connectionId,
            ExternalId = externalPortfolioId
        });

        return new PlatformPortfolioDto
        {
            ExternalId = platformPortfolio.ExternalId,
            Name = platformPortfolio.Name,
            Balance = platformPortfolio.Balance,
            Equity = platformPortfolio.Equity,
            Margin = platformPortfolio.Margin,
            FreeMargin = platformPortfolio.FreeMargin,
            Profit = platformPortfolio.Profit,
            Timestamp = platformPortfolio.Timestamp,
            CertusPortfolioId = portfolio.Id,
            Strategies = platformPortfolio.Strategies.Select(s => new PlatformStrategySummaryDto
            {
                ExternalId = s.ExternalId,
                Name = s.Name,
                IsActive = s.IsActive,
                Profit = s.Profit,
                TotalTrades = s.TotalTrades,
                LastTradeTime = s.LastTradeTime
            }).ToList()
        };
    }

    public async Task<List<PlatformStrategyDto>> GetStrategiesAsync(Guid connectionId, string portfolioExternalId)
    {
        var strategies = await _dataRepo.GetLatestStrategySnapshotsAsync(connectionId, portfolioExternalId);
        return strategies.Select(s => new PlatformStrategyDto
        {
            ExternalId = s.ExternalId,
            Name = s.Name,
            IsActive = s.IsActive,
            Profit = s.Profit,
            TotalTrades = s.TotalTrades,
            WinningTrades = s.WinningTrades,
            LosingTrades = s.LosingTrades,
            LastTradeTime = s.LastTradeTime,
            Timestamp = s.Timestamp
        }).ToList();
    }

    public async Task<ImportTradesResult> ImportTradesAsync(Guid connectionId, string strategyExternalId, DateTime? from = null, DateTime? to = null)
    {
        var connection = await _connectionRepo.GetByIdAsync(connectionId)
            ?? throw new InvalidOperationException($"Connection not found: {connectionId}");

        var trades = await _dataRepo.GetTradesByStrategyAsync(connectionId, strategyExternalId);
        
        var filteredTrades = trades
            .Where(t => from == null || t.OpenTime >= from)
            .Where(t => to == null || t.OpenTime <= to)
            .ToList();

        int imported = 0;
        int skipped = 0;
        decimal totalPnL = 0;
        var errors = new List<string>();

        foreach (var trade in filteredTrades)
        {
            try
            {
                var existing = await _tradeRepo.GetByExternalIdAsync(trade.ExternalId);
                if (existing != null)
                {
                    skipped++;
                    continue;
                }

                var strategyId = await FindOrCreateStrategyMappingAsync(connectionId, strategyExternalId);

                var importedTrade = new ImportedTrade
                {
                    Id = Guid.NewGuid(),
                    ExternalId = trade.ExternalId,
                    StrategyId = strategyId,
                    PortfolioId = Guid.Empty,
                    ConnectionId = connectionId,
                    Symbol = trade.Symbol,
                    Side = trade.Side.ToString(),
                    Volume = trade.Volume,
                    OpenPrice = trade.OpenPrice,
                    ClosePrice = trade.ClosePrice,
                    StopLoss = trade.StopLoss,
                    TakeProfit = trade.TakeProfit,
                    Profit = trade.Profit,
                    Commission = trade.Commission,
                    Swap = trade.Swap,
                    OpenTime = trade.OpenTime,
                    CloseTime = trade.CloseTime,
                    Comment = trade.Comment,
                    ImportedAt = DateTime.UtcNow
                };

                await _tradeRepo.AddAsync(importedTrade);
                imported++;
                totalPnL += importedTrade.PnL;

                await _eventDispatcher.DispatchAsync(new TradeImported
                {
                    TradeId = importedTrade.Id,
                    StrategyId = strategyId,
                    ExternalId = trade.ExternalId,
                    PnL = importedTrade.PnL
                });
            }
            catch (Exception ex)
            {
                errors.Add($"Trade {trade.ExternalId}: {ex.Message}");
            }
        }

        return new ImportTradesResult
        {
            TradesImported = imported,
            TradesSkipped = skipped,
            TotalPnL = totalPnL,
            EarliestTrade = filteredTrades.FirstOrDefault()?.OpenTime,
            LatestTrade = filteredTrades.LastOrDefault()?.OpenTime,
            Errors = errors
        };
    }

    public async Task<PlatformDashboardDto> GetDashboardAsync()
    {
        var connections = await _connectionRepo.GetAllAsync();

        foreach (var conn in connections)
            CheckStaleness(conn);

        var connectionDtos = connections.Select(c => MapToDto(c, _statusStore.Get(c.Id))).ToList();

        int totalTrades = 0;
        decimal totalPnL = 0;
        foreach (var conn in connections)
        {
            totalTrades += await _tradeRepo.GetCountByStrategyAsync(Guid.Empty);
            totalPnL += await _tradeRepo.GetTotalPnLByStrategyAsync(Guid.Empty);
        }

        var activeCount = connections.Count(c =>
            _statusStore.Get(c.Id).State == PlatformConnectionStatus.Connected);

        return new PlatformDashboardDto
        {
            TotalConnections = connections.Count,
            ActiveConnections = activeCount,
            TotalPortfolios = (await _portfolioRepo.GetAllAsync()).Count(p => p.IsPlatformManaged),
            TotalTrades = totalTrades,
            TotalPnL = totalPnL,
            Connections = connectionDtos
        };
    }

    private async Task<Guid> FindOrCreateStrategyMappingAsync(Guid connectionId, string strategyExternalId)
    {
        var existingTrades = await _tradeRepo.GetByStrategyIdAsync(Guid.Empty);
        var existing = existingTrades.FirstOrDefault(t => t.ExternalId == strategyExternalId);
        if (existing != null)
            return existing.StrategyId;

        return Guid.Empty;
    }

    private async Task AutoImportFromChangeAsync(PlatformConnection connection, FileImportEventArgs args)
    {
        try
        {
            var plugin = _pluginLoader.GetPlugin(connection.PlatformId);
            if (plugin == null) return;

            var parser = plugin.GetParsers().FirstOrDefault();
            if (parser == null) return;

            var content = await _fileImportService.ReadFileAsync(args.FilePath);
            if (string.IsNullOrEmpty(content)) return;

            var fileName = Path.GetFileName(args.FilePath).ToLowerInvariant();

            if (fileName.Contains("portfolio"))
            {
                var platformPortfolio = parser.ParsePortfolio(content);
                if (platformPortfolio != null)
                {
                    var existingPortfolios = await _portfolioRepo.GetAllAsync();
                    var existing = existingPortfolios.FirstOrDefault(p =>
                        p.PlatformConnectionId == connection.Id &&
                        p.ExternalPortfolioId == platformPortfolio.ExternalId);

                    if (existing == null)
                    {
                        var portfolio = new Portfolio(
                            Guid.NewGuid(),
                            platformPortfolio.Name,
                            0, 0,
                            new Money(platformPortfolio.Balance, Currency.USD),
                            $"Auto-imported from {connection.PlatformName}");

                        portfolio.SetPlatformReference(connection.Id, platformPortfolio.ExternalId);
                        await _portfolioRepo.AddAsync(portfolio);

                        await _eventDispatcher.DispatchAsync(new PortfolioImported
                        {
                            PortfolioId = portfolio.Id,
                            ConnectionId = connection.Id,
                            ExternalId = platformPortfolio.ExternalId
                        });
                    }
                    else
                    {
                        existing.UpdateCapital(new Money(platformPortfolio.Balance, Currency.USD));
                        existing.UpdateLastSynced();
                        _portfolioRepo.Update(existing);
                    }
                }
            }
            else if (fileName.Contains("trade"))
            {
                var platformTrades = parser.ParseTrades(content);
                foreach (var platformTrade in platformTrades)
                {
                    var existing = await _tradeRepo.GetByExternalIdAsync(platformTrade.ExternalId);
                    if (existing != null) continue;

                    var strategyId = await FindOrCreateStrategyMappingAsync(connection.Id, platformTrade.StrategyExternalId);

                    var importedTrade = new ImportedTrade
                    {
                        Id = Guid.NewGuid(),
                        ExternalId = platformTrade.ExternalId,
                        StrategyId = strategyId,
                        PortfolioId = Guid.Empty,
                        ConnectionId = connection.Id,
                        Symbol = platformTrade.Symbol,
                        Side = platformTrade.Side.ToString(),
                        Volume = platformTrade.Volume,
                        OpenPrice = platformTrade.OpenPrice,
                        ClosePrice = platformTrade.ClosePrice,
                        StopLoss = platformTrade.StopLoss,
                        TakeProfit = platformTrade.TakeProfit,
                        Profit = platformTrade.Profit,
                        Commission = platformTrade.Commission,
                        Swap = platformTrade.Swap,
                        OpenTime = platformTrade.OpenTime,
                        CloseTime = platformTrade.CloseTime,
                        Comment = platformTrade.Comment,
                        ImportedAt = DateTime.UtcNow
                    };

                    await _tradeRepo.AddAsync(importedTrade);

                    await _eventDispatcher.DispatchAsync(new TradeImported
                    {
                        TradeId = importedTrade.Id,
                        StrategyId = strategyId,
                        ExternalId = platformTrade.ExternalId,
                        PnL = importedTrade.PnL
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PlatformService] Auto-import error: {ex.Message}");
        }
    }

    private static PlatformConnectionDto MapToDto(PlatformConnection connection, ConnectionStatus status)
    {
        return new PlatformConnectionDto
        {
            Id = connection.Id,
            PlatformId = connection.PlatformId,
            PlatformName = connection.PlatformName,
            Status = status.State,
            ConnectedAt = status.ConnectedAt?.ToLocalTime(),
            LastDataReceivedAt = status.LastDataReceivedAt?.ToLocalTime(),
            ErrorMessage = status.ErrorMessage,
            RetryCount = status.RetryCount
        };
    }

    private void CheckStaleness(PlatformConnection connection)
    {
        var status = _statusStore.Get(connection.Id);
        if (string.IsNullOrEmpty(connection.Config.FilePath))
            return;

        // Determine the actual file path
        var filePath = File.Exists(connection.Config.FilePath)
            ? connection.Config.FilePath
            : Path.Combine(connection.Config.FilePath, "portfolio_status.json");

        if (!File.Exists(filePath))
        {
            if (status.State == PlatformConnectionStatus.Connected)
            {
                _statusStore.Set(connection.Id, status.WithError(
                    $"Data file not found: {filePath}. Ensure the EA is running on MetaTrader."));
            }
            return;
        }

        // Get last modified time from the actual file on disk
        var lastModified = File.GetLastWriteTimeUtc(filePath);
        var staleThreshold = TimeSpan.FromSeconds(connection.Config.StaleThresholdSeconds);
        var isStale = DateTime.UtcNow - lastModified > staleThreshold;

        if (isStale && status.State == PlatformConnectionStatus.Connected)
        {
            _statusStore.Set(connection.Id, status.WithError(
                $"No data received for {connection.Config.StaleThresholdSeconds}s. File may be stale."));
        }
        else if (!isStale && status.State != PlatformConnectionStatus.Connected)
        {
            // File is back and fresh — recover to connected
            _statusStore.Set(connection.Id, ConnectionStatus.Disconnected.WithConnected());
        }
    }
}
