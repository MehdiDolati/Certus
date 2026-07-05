using Certus.Application.Strategies;
using Certus.Application.Strategies.DTOs;
using Certus.Application.Trading.DTOs;
using Certus.Domain.RiskAndPortfolio.Enums;
using Certus.Domain.SharedKernel;
using Certus.Domain.Strategy.Enums;
using Certus.Domain.Strategy.Aggregates;
using Certus.Domain.Strategy.Entities;
using Certus.Domain.Strategy.ValueObjects;
using Certus.Domain.Execution.Enums;
using Certus.Domain.Execution.Entities;
using Certus.Domain.Evaluation.Entities;
using FluentAssertions;
using Moq;

namespace Certus.Application.Tests;

public class StrategyServiceTests
{
    private readonly Mock<Certus.Domain.Strategy.Repositories.IStrategyDefinitionRepository> _strategyRepo = new();
    private readonly Mock<Certus.Domain.Execution.Repositories.ITradeRepository> _tradeRepo = new();
    private readonly Mock<Certus.Domain.RiskAndPortfolio.Repositories.IPortfolioRepository> _portfolioRepo = new();
    private readonly Mock<Certus.Domain.Evaluation.Repositories.IPerformanceSnapshotRepository> _snapshotRepo = new();

    [Fact]
    public async Task GetStrategiesByPortfolioAsync_Should_Return_Empty_When_No_Strategies()
    {
        _strategyRepo.Setup(r => r.GetByPortfolioIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<StrategyDefinition>());
        var service = CreateService();

        var result = await service.GetStrategiesByPortfolioAsync(Guid.NewGuid(), DateRange.All);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetStrategiesByPortfolioAsync_Should_Return_Strategies_With_KPIs()
    {
        var portfolioId = Guid.NewGuid();
        var strategyId = Guid.NewGuid();
        var strategy = CreateStrategyWithSlot(strategyId, "Momentum", portfolioId, weight: 0.6m);
        _strategyRepo.Setup(r => r.GetByPortfolioIdAsync(portfolioId))
            .ReturnsAsync(new List<StrategyDefinition> { strategy });

        var snapshots = new List<PerformanceSnapshot>
        {
            CreateSnapshot(portfolioId, strategyId, actualReturn: 0.15m, supposedReturn: 0.12m, sharpe: 1.8m)
        };
        _snapshotRepo.Setup(r => r.GetByStrategyIdAsync(strategyId, It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(snapshots);

        var trades = new List<Domain.Execution.Aggregates.Trade>
        {
            CreateClosedTrade(strategyId, portfolioId, "BTC", TradeSide.Long, 100m, 110m, 1m),
            CreateClosedTrade(strategyId, portfolioId, "ETH", TradeSide.Long, 200m, 190m, 1m),
        };
        _tradeRepo.Setup(r => r.GetByStrategyIdAsync(strategyId)).ReturnsAsync(trades);

        var service = CreateService();

        var result = await service.GetStrategiesByPortfolioAsync(portfolioId, DateRange.All);

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Momentum");
        result[0].ActualReturn.Should().Be(0.15m);
        result[0].SupposedReturn.Should().Be(0.12m);
        result[0].Variance.Should().Be(0.03m);
        result[0].Sharpe.Should().Be(1.8m);
        result[0].ContributionPercent.Should().Be(60m);
        result[0].TradeCount.Should().Be(2);
        result[0].WinRate.Should().Be(0.5m);
    }

    [Fact]
    public async Task GetStrategyPerformanceAsync_Should_Return_Empty_When_No_Snapshots()
    {
        _snapshotRepo.Setup(r => r.GetByStrategyIdAsync(It.IsAny<Guid>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(new List<PerformanceSnapshot>());
        var service = CreateService();

        var result = await service.GetStrategyPerformanceAsync(Guid.NewGuid(), DateRange.All);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetStrategyPerformanceAsync_Should_Return_Ordered_Snapshots()
    {
        var strategyId = Guid.NewGuid();
        var snapshots = new List<PerformanceSnapshot>
        {
            CreateSnapshot(Guid.NewGuid(), strategyId, date: new DateOnly(2024, 1, 3), actualReturn: 0.3m),
            CreateSnapshot(Guid.NewGuid(), strategyId, date: new DateOnly(2024, 1, 1), actualReturn: 0.1m),
            CreateSnapshot(Guid.NewGuid(), strategyId, date: new DateOnly(2024, 1, 2), actualReturn: 0.2m),
        };
        _snapshotRepo.Setup(r => r.GetByStrategyIdAsync(strategyId, It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(snapshots);

        var service = CreateService();

        var result = await service.GetStrategyPerformanceAsync(strategyId, DateRange.All);

        result.Should().HaveCount(3);
        result[0].ActualReturn.Should().Be(0.1m);
        result[1].ActualReturn.Should().Be(0.2m);
        result[2].ActualReturn.Should().Be(0.3m);
    }

    private StrategyService CreateService()
    {
        return new StrategyService(
            _strategyRepo.Object,
            _tradeRepo.Object,
            _portfolioRepo.Object,
            _snapshotRepo.Object);
    }

    private static StrategyDefinition CreateStrategyWithSlot(Guid strategyId, string name, Guid portfolioId, decimal weight = 0.5m)
    {
        var strategy = new StrategyDefinition(
            strategyId, name, new StrategyType(StrategyCategory.Momentum), 0.15m);
        strategy.AssignToPortfolio(portfolioId, new Weight(weight));
        return strategy;
    }

    private static PerformanceSnapshot CreateSnapshot(
        Guid portfolioId, Guid? strategyId = null,
        DateOnly? date = null,
        decimal actualReturn = 0m, decimal supposedReturn = 0m,
        decimal sharpe = 0m, decimal drawdown = 0m,
        decimal equity = 0m, decimal dailyPnL = 0m)
    {
        return new PerformanceSnapshot(
            Guid.NewGuid(), Guid.NewGuid(), portfolioId, strategyId,
            date ?? new DateOnly(2024, 1, 1),
            actualReturn, supposedReturn, dailyPnL, equity, drawdown, sharpe);
    }

    private static Domain.Execution.Aggregates.Trade CreateClosedTrade(
        Guid strategyId, Guid portfolioId, string symbol, TradeSide side,
        decimal entryPrice, decimal exitPrice, decimal quantity)
    {
        var tradeId = Guid.NewGuid();
        var sym = new Symbol(symbol, AssetClass.Crypto);
        var trade = new Domain.Execution.Aggregates.Trade(tradeId, strategyId, portfolioId, sym, side, entryPrice, quantity);

        var entryOrder = new Order(
            Guid.NewGuid(), tradeId, symbol, side, OrderType.Market, quantity, ExecutionVenue.Simulated);
        entryOrder.Fill(quantity, entryPrice, 0m);
        trade.Open(entryOrder);

        var exitSide = side == TradeSide.Long ? TradeSide.Short : TradeSide.Long;
        var exitOrder = new Order(
            Guid.NewGuid(), tradeId, symbol, exitSide, OrderType.Market, quantity, ExecutionVenue.Simulated);
        exitOrder.Fill(quantity, exitPrice, 0m);
        trade.Close(exitOrder);

        return trade;
    }
}
