using Certus.Application.Trading.DTOs;
using Certus.Domain.SharedKernel;
using Certus.Domain.Execution.Enums;
using Certus.Domain.Execution.Repositories;
using Certus.Domain.Evaluation.Repositories;

namespace Certus.Application.Trading;

public class TradeService : ITradeService
{
    private readonly ITradeRepository _tradeRepository;
    private readonly IPerformanceSnapshotRepository _performanceSnapshotRepository;

    public TradeService(ITradeRepository tradeRepository, IPerformanceSnapshotRepository performanceSnapshotRepository)
    {
        _tradeRepository = tradeRepository;
        _performanceSnapshotRepository = performanceSnapshotRepository;
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

        var items = trades.Select(t => new TradeDto(
            t.Id,
            string.Empty,
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

        var tradeDto = new TradeDto(
            trade.Id,
            string.Empty,
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

        return new TradeDetailDto(tradeDto, trade.AgentReason, 0m, 0m);
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
}
