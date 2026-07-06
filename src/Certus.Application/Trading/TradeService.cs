using Certus.Application.Trading.DTOs;
using Certus.Domain.SharedKernel;
using Certus.Domain.Execution.Enums;
using Certus.Domain.Execution.Repositories;
using Certus.Domain.Evaluation.Repositories;
using Certus.Domain.Strategy.Repositories;

namespace Certus.Application.Trading;

public class TradeService : ITradeService
{
    private readonly ITradeRepository _tradeRepository;
    private readonly IPerformanceSnapshotRepository _performanceSnapshotRepository;
    private readonly IStrategyDefinitionRepository _strategyRepository;

    public TradeService(
        ITradeRepository tradeRepository,
        IPerformanceSnapshotRepository performanceSnapshotRepository,
        IStrategyDefinitionRepository strategyRepository)
    {
        _tradeRepository = tradeRepository;
        _performanceSnapshotRepository = performanceSnapshotRepository;
        _strategyRepository = strategyRepository;
    }

    public async Task<PaginatedResult<TradeDto>> GetTradesAsync(TradeFilter filter)
    {
        var dateFrom = filter.Period?.From;
        var dateTo = filter.Period?.To;

        Symbol? symbolFilter = filter.Symbol is not null ? new Symbol(filter.Symbol, AssetClass.Crypto) : null;

        var totalCount = await _tradeRepository.CountFilteredAsync(
            filter.PortfolioId, filter.StrategyId, symbolFilter,
            filter.Side, filter.Status, dateFrom, dateTo, filter.Search);

        var trades = await _tradeRepository.GetFilteredAsync(
            filter.PortfolioId, filter.StrategyId, symbolFilter,
            filter.Side, filter.Status, dateFrom, dateTo, filter.Search,
            filter.Page, filter.PageSize);

        var strategyNames = await ResolveStrategyNames(trades.Select(t => t.StrategyId).Distinct());

        var items = trades.Select(t => new TradeDto(
            t.Id,
            strategyNames.GetValueOrDefault(t.StrategyId, string.Empty),
            t.Symbol.Value,
            t.Side,
            t.EntryTime,
            t.ExitTime,
            t.EntryPrice,
            t.ExitPrice,
            t.Quantity,
            t.PnL,
            t.PnLPercent,
            t.Duration,
            t.Fees,
            t.Slippage,
            t.Status)).ToList();

        return new PaginatedResult<TradeDto>(items, totalCount, filter.Page, filter.PageSize);
    }

    public async Task<TradeDetailDto?> GetTradeDetailAsync(Guid tradeId)
    {
        var trade = await _tradeRepository.GetByIdAsync(tradeId);
        if (trade is null) return null;

        var strategyNames = await ResolveStrategyNames([trade.StrategyId]);
        var strategyName = strategyNames.GetValueOrDefault(trade.StrategyId, string.Empty);

        var tradeDto = new TradeDto(
            trade.Id,
            strategyName,
            trade.Symbol.Value,
            trade.Side,
            trade.EntryTime,
            trade.ExitTime,
            trade.EntryPrice,
            trade.ExitPrice,
            trade.Quantity,
            trade.PnL,
            trade.PnLPercent,
            trade.Duration,
            trade.Fees,
            trade.Slippage,
            trade.Status);

        var portfolioTrades = await _tradeRepository.GetByPortfolioIdAsync(trade.PortfolioId);
        var portfolioImpact = portfolioTrades.Where(t => t.Status == TradeStatus.Closed).Sum(t => t.PnL);

        var strategyTrades = await _tradeRepository.GetByStrategyIdAsync(trade.StrategyId);
        var strategyImpact = strategyTrades.Where(t => t.Status == TradeStatus.Closed).Sum(t => t.PnL);

        return new TradeDetailDto(tradeDto, trade.AgentReason, portfolioImpact, strategyImpact);
    }

    public async Task<List<PerformanceSnapshotDto>> GetCumulativePnlAsync(
        Guid? portfolioId, Guid? strategyId, DateRange period)
    {
        IReadOnlyList<Domain.Evaluation.Entities.PerformanceSnapshot> snapshots;

        if (portfolioId.HasValue)
            snapshots = await _performanceSnapshotRepository.GetByPortfolioIdAsync(portfolioId.Value, period?.From, period?.To);
        else if (strategyId.HasValue)
            snapshots = await _performanceSnapshotRepository.GetByStrategyIdAsync(strategyId.Value, period?.From, period?.To);
        else
            snapshots = [];

        var orderedSnapshots = snapshots.OrderBy(s => s.Date).ToList();

        var cumulative = 0m;
        return orderedSnapshots.Select(s =>
        {
            cumulative += s.DailyPnL;
            return new PerformanceSnapshotDto(
                s.Date, s.ActualReturn, s.SupposedReturn, cumulative, s.Equity, s.Drawdown, s.Sharpe);
        }).ToList();
    }

    private async Task<Dictionary<Guid, string>> ResolveStrategyNames(IEnumerable<Guid> strategyIds)
    {
        var idList = strategyIds.Where(id => id != Guid.Empty).ToList();
        if (idList.Count == 0) return [];

        var strategies = new List<Domain.Strategy.Aggregates.StrategyDefinition>();
        foreach (var id in idList)
        {
            var s = await _strategyRepository.GetByIdAsync(id);
            if (s != null) strategies.Add(s);
        }

        return strategies.ToDictionary(s => s.Id, s => s.Name);
    }
}
