using Certus.Application.Strategies.DTOs;
using Certus.Application.Trading.DTOs;
using Certus.Domain.RiskAndPortfolio.Repositories;
using Certus.Domain.Strategy.Aggregates;
using Certus.Domain.Strategy.Enums;
using Certus.Domain.Strategy.Repositories;
using Certus.Domain.Strategy.ValueObjects;
using Certus.Domain.Execution.Enums;
using Certus.Domain.Execution.Repositories;
using Certus.Domain.Evaluation.Entities;
using Certus.Domain.Evaluation.Repositories;
using Certus.Domain.SharedKernel;

namespace Certus.Application.Strategies;

public class StrategyService : IStrategyService
{
    private readonly IStrategyDefinitionRepository _strategyRepository;
    private readonly ITradeRepository _tradeRepository;
    private readonly IPortfolioRepository _portfolioRepository;
    private readonly IPerformanceSnapshotRepository _performanceSnapshotRepository;

    public StrategyService(
        IStrategyDefinitionRepository strategyRepository,
        ITradeRepository tradeRepository,
        IPortfolioRepository portfolioRepository,
        IPerformanceSnapshotRepository performanceSnapshotRepository)
    {
        _strategyRepository = strategyRepository;
        _tradeRepository = tradeRepository;
        _portfolioRepository = portfolioRepository;
        _performanceSnapshotRepository = performanceSnapshotRepository;
    }

    public async Task<List<StrategyPerformanceDto>> GetStrategiesByPortfolioAsync(Guid portfolioId, DateRange period)
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

    public async Task<List<PerformanceSnapshotDto>> GetStrategyPerformanceAsync(Guid strategyId, DateRange period)
    {
        var snapshots = await _performanceSnapshotRepository.GetByStrategyIdAsync(strategyId, period?.From, period?.To);

        return snapshots.OrderBy(s => s.Date)
            .Select(s => new PerformanceSnapshotDto(
                s.Date, s.ActualReturn, s.SupposedReturn, s.DailyPnL, s.Equity, s.Drawdown, s.Sharpe))
            .ToList();
    }

    public async Task<StrategyPerformanceDto> CreateAsync(CreateStrategyRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Strategy name is required.");
        if (request.Weight < 0m || request.Weight > 1m)
            throw new ArgumentException("Weight must be between 0.0 and 1.0.");

        var portfolio = await _portfolioRepository.GetByIdAsync(request.PortfolioId);
        if (portfolio is null)
            throw new InvalidOperationException("Portfolio not found.");
        if (!portfolio.IsActive)
            throw new InvalidOperationException("Cannot add strategy to non-active portfolio.");

        var strategyType = new StrategyType(StrategyCategory.Custom, request.Type);

        var id = Guid.NewGuid();
        var strategy = new StrategyDefinition(
            id,
            request.Name.Trim(),
            strategyType,
            request.TargetReturn,
            description: "");

        strategy.AssignToPortfolio(request.PortfolioId, new Weight(request.Weight));

        await _strategyRepository.AddAsync(strategy);
        await _strategyRepository.SaveChangesAsync();

        return new StrategyPerformanceDto(
            strategy.Id, strategy.Name, strategy.Type.ToString(), request.Weight * 100m,
            0m, 0m, 0m, 0m, 0, 0m, strategy.Status);
    }

    public async Task<StrategyPerformanceDto?> UpdateAsync(Guid id, UpdateStrategyRequest request)
    {
        var strategy = await _strategyRepository.GetByIdAsync(id);
        if (strategy is null) return null;

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Strategy name is required.");
        if (request.Weight < 0m || request.Weight > 1m)
            throw new ArgumentException("Weight must be between 0.0 and 1.0.");

        var strategyType = new StrategyType(StrategyCategory.Custom, request.Type);
        strategy.UpdateDetails(request.Name.Trim(), strategyType, request.TargetReturn, description: "");

        foreach (var slot in strategy.Slots)
        {
            slot.UpdateWeight(new Weight(request.Weight));
        }

        _strategyRepository.Update(strategy);
        await _strategyRepository.SaveChangesAsync();

        return new StrategyPerformanceDto(
            strategy.Id, strategy.Name, strategy.Type.ToString(), request.Weight * 100m,
            0m, 0m, 0m, 0m, 0, 0m, strategy.Status);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var strategy = await _strategyRepository.GetByIdAsync(id);
        if (strategy is null) return false;

        strategy.Retire();

        _strategyRepository.Update(strategy);
        await _strategyRepository.SaveChangesAsync();

        return true;
    }
}
