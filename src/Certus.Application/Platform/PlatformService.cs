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
    private readonly IDomainEventDispatcher _eventDispatcher;

    public PlatformService(
        IPlatformPluginLoader pluginLoader,
        IPlatformConnectionRepository connectionRepo,
        IPlatformDataRepository dataRepo,
        IPortfolioRepository portfolioRepo,
        IImportedTradeRepository tradeRepo,
        IFileImportService fileImportService,
        IDomainEventDispatcher eventDispatcher)
    {
        _pluginLoader = pluginLoader;
        _connectionRepo = connectionRepo;
        _dataRepo = dataRepo;
        _portfolioRepo = portfolioRepo;
        _tradeRepo = tradeRepo;
        _fileImportService = fileImportService;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<PlatformConnectionDto> ConnectAsync(ConnectPlatformRequest request)
    {
        var plugin = _pluginLoader.GetPluginForPlatform(request.PlatformId)
            ?? throw new InvalidOperationException($"No plugin found for platform: {request.PlatformId}");

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

        var adapter = plugin.CreateAdapter(config);
        var result = await adapter.ConnectAsync(config);

        if (!result.Success)
            throw new InvalidOperationException($"Failed to connect: {result.ErrorMessage}");

        var connection = PlatformConnection.Create(
            request.PlatformId,
            plugin.PluginName,
            config);

        connection.Connect();

        await _connectionRepo.AddAsync(connection);

        if (!string.IsNullOrEmpty(request.FilePath) && request.UseFileWatcher)
        {
            _fileImportService.StartWatching(connection.Id, request.FilePath);
            _fileImportService.FileChanged += (sender, args) =>
            {
                if (args.ConnectionId == connection.Id)
                {
                    connection.UpdateLastDataReceived();
                }
            };
        }

        await _eventDispatcher.DispatchAsync(new PlatformConnected
        {
            ConnectionId = connection.Id,
            PlatformId = request.PlatformId
        });

        return MapToDto(connection);
    }

    public async Task DisconnectAsync(Guid connectionId)
    {
        var connection = await _connectionRepo.GetByIdAsync(connectionId)
            ?? throw new InvalidOperationException($"Connection not found: {connectionId}");

        _fileImportService.StopWatching(connectionId);
        connection.Disconnect("User disconnected");

        _connectionRepo.Update(connection);
    }

    public async Task<List<PlatformConnectionDto>> GetConnectionsAsync()
    {
        var connections = await _connectionRepo.GetAllAsync();
        return connections.Select(MapToDto).ToList();
    }

    public async Task<PlatformConnectionDto?> GetConnectionAsync(Guid connectionId)
    {
        var connection = await _connectionRepo.GetByIdAsync(connectionId);
        return connection == null ? null : MapToDto(connection);
    }

    public async Task<PlatformStatusDto> GetStatusAsync(Guid connectionId)
    {
        var connection = await _connectionRepo.GetByIdAsync(connectionId)
            ?? throw new InvalidOperationException($"Connection not found: {connectionId}");

        var portfolios = await _dataRepo.GetLatestPortfolioSnapshotAsync(connectionId, string.Empty);
        var tradeCount = await _tradeRepo.GetCountByStrategyAsync(Guid.Empty);

        return new PlatformStatusDto
        {
            ConnectionId = connection.Id,
            PlatformName = connection.PlatformName,
            Status = connection.Status,
            IsConnected = connection.Status == PlatformConnectionStatus.Connected,
            ConnectedAt = connection.ConnectedAt,
            LastDataReceivedAt = connection.LastDataReceivedAt,
            ErrorMessage = connection.ErrorMessage
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

                // Find or create strategy mapping
                var strategyId = await FindOrCreateStrategyMappingAsync(connectionId, strategyExternalId);

                var importedTrade = new ImportedTrade
                {
                    Id = Guid.NewGuid(),
                    ExternalId = trade.ExternalId,
                    StrategyId = strategyId,
                    PortfolioId = Guid.Empty, // Will be set when portfolio is imported
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
        var connectionDtos = connections.Select(MapToDto).ToList();

        int totalTrades = 0;
        decimal totalPnL = 0;
        foreach (var conn in connections)
        {
            totalTrades += await _tradeRepo.GetCountByStrategyAsync(Guid.Empty);
            totalPnL += await _tradeRepo.GetTotalPnLByStrategyAsync(Guid.Empty);
        }

        return new PlatformDashboardDto
        {
            TotalConnections = connections.Count,
            ActiveConnections = connections.Count(c => c.Status == PlatformConnectionStatus.Connected),
            TotalPortfolios = (await _portfolioRepo.GetAllAsync()).Count(p => p.IsPlatformManaged),
            TotalTrades = totalTrades,
            TotalPnL = totalPnL,
            Connections = connectionDtos
        };
    }

    private async Task<Guid> FindOrCreateStrategyMappingAsync(Guid connectionId, string strategyExternalId)
    {
        // Check if we already have a mapping for this strategy
        var existingTrades = await _tradeRepo.GetByStrategyIdAsync(Guid.Empty);
        var existing = existingTrades.FirstOrDefault(t => t.ExternalId == strategyExternalId);
        if (existing != null)
            return existing.StrategyId;

        // For now, return empty guid - in production, we'd create a strategy record
        return Guid.Empty;
    }

    private static PlatformConnectionDto MapToDto(PlatformConnection connection)
    {
        return new PlatformConnectionDto
        {
            Id = connection.Id,
            PlatformId = connection.PlatformId,
            PlatformName = connection.PlatformName,
            Status = connection.Status,
            ConnectedAt = connection.ConnectedAt,
            LastDataReceivedAt = connection.LastDataReceivedAt,
            ErrorMessage = connection.ErrorMessage,
            RetryCount = connection.RetryCount
        };
    }
}
