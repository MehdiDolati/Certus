using Certus.Application.Portfolios.DTOs;
using Certus.Application.Strategies.DTOs;
using Certus.Application.Trading.DTOs;
using Certus.Domain.RiskAndPortfolio.Aggregates;
using Certus.Domain.RiskAndPortfolio.Enums;
using Certus.Domain.RiskAndPortfolio.Repositories;
using Certus.Domain.Strategy.Repositories;
using Certus.Domain.Execution.Enums;
using Certus.Domain.Execution.Repositories;
using Certus.Domain.Evaluation.Entities;
using Certus.Domain.Evaluation.Repositories;
using Certus.Domain.SharedKernel;

namespace Certus.Application.Portfolios;

public class PortfolioService : IPortfolioService
{
    private readonly IPortfolioRepository _portfolioRepository;
    private readonly IStrategyDefinitionRepository _strategyRepository;
    private readonly ITradeRepository _tradeRepository;
    private readonly IPerformanceSnapshotRepository _performanceSnapshotRepository;

    public PortfolioService(
        IPortfolioRepository portfolioRepository,
        IStrategyDefinitionRepository strategyRepository,
        ITradeRepository tradeRepository,
        IPerformanceSnapshotRepository performanceSnapshotRepository)
    {
        _portfolioRepository = portfolioRepository;
        _strategyRepository = strategyRepository;
        _tradeRepository = tradeRepository;
        _performanceSnapshotRepository = performanceSnapshotRepository;
    }

    public async Task<List<PortfolioDto>> GetAllPortfoliosAsync(PortfolioFilter filter)
    {
        var portfolios = await _portfolioRepository.GetAllAsync();
        var filtered = portfolios.AsQueryable();

        if (filter.Status.HasValue)
            filtered = filtered.Where(p => p.Status == filter.Status.Value);

        if (filter.MinAum.HasValue)
            filtered = filtered.Where(p => p.AllocatedCapital.Amount >= filter.MinAum.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
            filtered = filtered.Where(p => p.Name.Contains(filter.Search) || p.Description.Contains(filter.Search));

        var dateFrom = filter.Period?.From;
        var dateTo = filter.Period?.To;

        var result = new List<PortfolioDto>();
        foreach (var p in filtered)
        {
            var snapshots = await _performanceSnapshotRepository.GetByPortfolioIdAsync(p.Id, dateFrom, dateTo);
            var orderedSnapshots = snapshots.OrderBy(s => s.Date).ToList();

            var actualReturn = orderedSnapshots.Count > 0 ? orderedSnapshots[^1].ActualReturn : 0m;
            var supposedReturn = orderedSnapshots.Count > 0 ? orderedSnapshots[^1].SupposedReturn : 0m;
            var variance = actualReturn - supposedReturn;
            var sharpe = orderedSnapshots.Count > 0 ? orderedSnapshots[^1].Sharpe : 0m;
            var maxDrawdown = orderedSnapshots.Count > 0 ? orderedSnapshots.Min(s => s.Drawdown) : 0m;

            var trades = await _tradeRepository.GetByPortfolioIdAsync(p.Id);
            var tradeCount = trades.Count;
            var closedTrades = trades.Where(t => t.Status == TradeStatus.Closed).ToList();
            var winCount = closedTrades.Count(t => t.PnL > 0);
            var winRate = closedTrades.Count > 0 ? (decimal)winCount / closedTrades.Count : 0m;

            result.Add(new PortfolioDto(
                p.Id, p.Name, p.Description, p.Status,
                actualReturn, supposedReturn, variance, sharpe, maxDrawdown,
                tradeCount, winRate, p.AllocatedCapital.Amount));
        }

        result = filter.SortBy switch
        {
            PortfolioSortBy.Name => filter.Ascending ? result.OrderBy(p => p.Name).ToList() : result.OrderByDescending(p => p.Name).ToList(),
            PortfolioSortBy.ActualReturn => filter.Ascending ? result.OrderBy(p => p.ActualReturn).ToList() : result.OrderByDescending(p => p.ActualReturn).ToList(),
            PortfolioSortBy.SupposedReturn => filter.Ascending ? result.OrderBy(p => p.SupposedReturn).ToList() : result.OrderByDescending(p => p.SupposedReturn).ToList(),
            PortfolioSortBy.Variance => filter.Ascending ? result.OrderBy(p => p.Variance).ToList() : result.OrderByDescending(p => p.Variance).ToList(),
            PortfolioSortBy.Sharpe => filter.Ascending ? result.OrderBy(p => p.Sharpe).ToList() : result.OrderByDescending(p => p.Sharpe).ToList(),
            PortfolioSortBy.MaxDrawdown => filter.Ascending ? result.OrderBy(p => p.MaxDrawdown).ToList() : result.OrderByDescending(p => p.MaxDrawdown).ToList(),
            PortfolioSortBy.TradeCount => filter.Ascending ? result.OrderBy(p => p.TradeCount).ToList() : result.OrderByDescending(p => p.TradeCount).ToList(),
            _ => result
        };

        return result;
    }

    public async Task<PortfolioDetailDto?> GetPortfolioDetailAsync(Guid id, DateRange period)
    {
        var portfolio = await _portfolioRepository.GetByIdAsync(id);
        if (portfolio is null) return null;

        var strategyDtos = await GetStrategiesForPortfolio(id, period);

        var snapshots = await _performanceSnapshotRepository.GetByPortfolioIdAsync(id, period?.From, period?.To);
        var orderedSnapshots = snapshots.OrderBy(s => s.Date).ToList();

        var actualReturn = orderedSnapshots.Count > 0 ? orderedSnapshots[^1].ActualReturn : 0m;
        var supposedReturn = orderedSnapshots.Count > 0 ? orderedSnapshots[^1].SupposedReturn : 0m;
        var variance = actualReturn - supposedReturn;
        var sharpe = orderedSnapshots.Count > 0 ? orderedSnapshots[^1].Sharpe : 0m;
        var maxDrawdown = orderedSnapshots.Count > 0 ? orderedSnapshots.Min(s => s.Drawdown) : 0m;

        var trades = await _tradeRepository.GetByPortfolioIdAsync(id);
        var tradeCount = trades.Count;
        var closedTrades = trades.Where(t => t.Status == TradeStatus.Closed).ToList();
        var winCount = closedTrades.Count(t => t.PnL > 0);
        var winRate = closedTrades.Count > 0 ? (decimal)winCount / closedTrades.Count : 0m;

        var summary = new PortfolioDto(
            portfolio.Id, portfolio.Name, portfolio.Description, portfolio.Status,
            actualReturn, supposedReturn, variance, sharpe, maxDrawdown,
            tradeCount, winRate, portfolio.AllocatedCapital.Amount);

        var equityCurve = orderedSnapshots.Select(s => new PerformanceSnapshotDto(
            s.Date, s.ActualReturn, s.SupposedReturn, s.DailyPnL, s.Equity, s.Drawdown, s.Sharpe)).ToList();

        var drawdownCurve = orderedSnapshots.Select(s => new PerformanceSnapshotDto(
            s.Date, s.ActualReturn, s.SupposedReturn, s.DailyPnL, s.Equity, s.Drawdown, s.Sharpe)).ToList();

        var monthlyReturns = orderedSnapshots
            .GroupBy(s => new { s.Date.Year, s.Date.Month })
            .Select(g => new MonthlyReturnDto(
                g.Key.Year, g.Key.Month,
                g.Max(s => s.ActualReturn) - g.Min(s => s.ActualReturn)))
            .ToList();

        return new PortfolioDetailDto(summary, strategyDtos, equityCurve, drawdownCurve, monthlyReturns);
    }

    public async Task<DashboardKpis> GetDashboardKpisAsync(DateRange period)
    {
        var portfolios = await _portfolioRepository.GetAllAsync();
        var latestByPortfolio = new List<PerformanceSnapshot>();

        foreach (var p in portfolios)
        {
            var snapshots = await _performanceSnapshotRepository.GetByPortfolioIdAsync(p.Id, period?.From, period?.To);

            if (snapshots.Count > 0)
            {
                var latest = snapshots.OrderByDescending(s => s.Date).First();
                latestByPortfolio.Add(latest);
            }
        }

        var totalAum = latestByPortfolio.Sum(s => s.Equity);
        var totalPnl = latestByPortfolio.Sum(s => s.DailyPnL);
        var avgSharpe = latestByPortfolio.Count > 0 ? latestByPortfolio.Average(s => s.Sharpe) : 0m;
        var maxDrawdown = latestByPortfolio.Count > 0 ? latestByPortfolio.Min(s => s.Drawdown) : 0m;

        var allTrades = new List<Domain.Execution.Aggregates.Trade>();
        foreach (var p in portfolios)
        {
            var trades = await _tradeRepository.GetByPortfolioIdAsync(p.Id);
            allTrades.AddRange(trades);
        }

        var closedTrades = allTrades.Where(t => t.Status == TradeStatus.Closed).ToList();
        var winRate = closedTrades.Count > 0 ? (decimal)closedTrades.Count(t => t.PnL > 0) / closedTrades.Count : 0m;

        var activeCount = portfolios.Count(p => p.Status == PortfolioStatus.Active);

        return new DashboardKpis(totalAum, totalPnl, avgSharpe, maxDrawdown, winRate, activeCount);
    }

    public async Task<List<PerformanceSnapshotDto>> GetPerformanceHistoryAsync(Guid id, DateRange period)
    {
        var snapshots = await _performanceSnapshotRepository.GetByPortfolioIdAsync(id, period?.From, period?.To);

        return snapshots.OrderBy(s => s.Date)
            .Select(s => new PerformanceSnapshotDto(
                s.Date, s.ActualReturn, s.SupposedReturn, s.DailyPnL, s.Equity, s.Drawdown, s.Sharpe))
            .ToList();
    }

    public async Task<PortfolioDto> CreateAsync(CreatePortfolioRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Portfolio name is required.");

        var id = Guid.NewGuid();
        var portfolio = new Portfolio(
            id,
            request.Name.Trim(),
            request.TargetReturn,
            request.TargetSharpe,
            new Money(request.AllocatedCapital, Currency.USD),
            request.Description);

        await _portfolioRepository.AddAsync(portfolio);
        await _portfolioRepository.SaveChangesAsync();

        return MapToDto(portfolio, 0, 0m, 0m, 0m, 0m, 0m);
    }

    public async Task<PortfolioDto?> UpdateAsync(Guid id, UpdatePortfolioRequest request)
    {
        var portfolio = await _portfolioRepository.GetByIdAsync(id);
        if (portfolio is null) return null;

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Portfolio name is required.");

        portfolio.UpdateDetails(request.Name.Trim(), request.Description, request.TargetReturn, request.TargetSharpe);
        portfolio.UpdateCapital(new Money(request.AllocatedCapital, Currency.USD));

        _portfolioRepository.Update(portfolio);
        await _portfolioRepository.SaveChangesAsync();

        return MapToDto(portfolio, 0, 0m, 0m, 0m, 0m, 0m);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var portfolio = await _portfolioRepository.GetByIdAsync(id);
        if (portfolio is null) return false;

        portfolio.Close();

        _portfolioRepository.Update(portfolio);
        await _portfolioRepository.SaveChangesAsync();

        return true;
    }

    private async Task<List<StrategyPerformanceDto>> GetStrategiesForPortfolio(Guid portfolioId, DateRange period)
    {
        var strategies = await _strategyRepository.GetByPortfolioIdAsync(portfolioId);
        var result = new List<StrategyPerformanceDto>();

        foreach (var s in strategies)
        {
            var slot = s.Slots.FirstOrDefault(sl => sl.PortfolioId == portfolioId);
            var weight = slot?.Weight?.Value ?? 0m;

            var snapshots = await _performanceSnapshotRepository.GetByStrategyIdAsync(s.Id, period?.From, period?.To);
            var orderedSnapshots = snapshots.OrderBy(ps => ps.Date).ToList();

            var actualReturn = orderedSnapshots.Count > 0 ? orderedSnapshots[^1].ActualReturn : 0m;
            var supposedReturn = orderedSnapshots.Count > 0 ? orderedSnapshots[^1].SupposedReturn : 0m;
            var sharpe = orderedSnapshots.Count > 0 ? orderedSnapshots[^1].Sharpe : 0m;

            var trades = await _tradeRepository.GetByStrategyIdAsync(s.Id);
            var tradeCount = trades.Count;
            var closedTrades = trades.Where(t => t.Status == TradeStatus.Closed).ToList();
            var winCount = closedTrades.Count(t => t.PnL > 0);
            var winRate = closedTrades.Count > 0 ? (decimal)winCount / closedTrades.Count : 0m;

            result.Add(new StrategyPerformanceDto(
                s.Id, s.Name, s.Type.ToString(), weight * 100m,
                actualReturn, supposedReturn, actualReturn - supposedReturn,
                sharpe, tradeCount, winRate, s.Status));
        }

        return result;
    }

    private static PortfolioDto MapToDto(Portfolio p, int tradeCount, decimal actualReturn, decimal supposedReturn, decimal sharpe, decimal maxDrawdown, decimal winRate)
    {
        return new PortfolioDto(
            p.Id, p.Name, p.Description, p.Status,
            actualReturn, supposedReturn, actualReturn - supposedReturn,
            sharpe, maxDrawdown, tradeCount, winRate, p.AllocatedCapital.Amount);
    }
}
