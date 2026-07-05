using Certus.Application.Portfolios;
using Certus.Application.Portfolios.DTOs;
using Certus.Application.Strategies.DTOs;
using Certus.Application.Trading.DTOs;
using Certus.Domain.RiskAndPortfolio.Aggregates;
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

public class PortfolioServiceTests
{
    private readonly Mock<Certus.Domain.RiskAndPortfolio.Repositories.IPortfolioRepository> _portfolioRepo = new();
    private readonly Mock<Certus.Domain.Strategy.Repositories.IStrategyDefinitionRepository> _strategyRepo = new();
    private readonly Mock<Certus.Domain.Execution.Repositories.ITradeRepository> _tradeRepo = new();
    private readonly Mock<Certus.Domain.Evaluation.Repositories.IPerformanceSnapshotRepository> _snapshotRepo = new();

    [Fact]
    public async Task GetAllPortfoliosAsync_Should_Return_Empty_When_No_Portfolios()
    {
        _portfolioRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Portfolio>());
        var service = CreateService();

        var result = await service.GetAllPortfoliosAsync(new PortfolioFilter());

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllPortfoliosAsync_Should_Filter_By_Status()
    {
        var active = CreatePortfolio("Active Fund", PortfolioStatus.Active);
        var closed = CreatePortfolio("Closed Fund", PortfolioStatus.Closed);
        _portfolioRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Portfolio> { active, closed });
        _snapshotRepo.Setup(r => r.GetByPortfolioIdAsync(It.IsAny<Guid>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(new List<PerformanceSnapshot>());
        _tradeRepo.Setup(r => r.GetByPortfolioIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<Domain.Execution.Aggregates.Trade>());

        var service = CreateService();

        var result = await service.GetAllPortfoliosAsync(new PortfolioFilter(Status: PortfolioStatus.Active));

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Active Fund");
    }

    [Fact]
    public async Task GetAllPortfoliosAsync_Should_Filter_By_MinAum()
    {
        var small = CreatePortfolio("Small", PortfolioStatus.Active, 100_000m);
        var large = CreatePortfolio("Large", PortfolioStatus.Active, 5_000_000m);
        _portfolioRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Portfolio> { small, large });
        _snapshotRepo.Setup(r => r.GetByPortfolioIdAsync(It.IsAny<Guid>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(new List<PerformanceSnapshot>());
        _tradeRepo.Setup(r => r.GetByPortfolioIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<Domain.Execution.Aggregates.Trade>());

        var service = CreateService();

        var result = await service.GetAllPortfoliosAsync(new PortfolioFilter(MinAum: 1_000_000m));

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Large");
    }

    [Fact]
    public async Task GetAllPortfoliosAsync_Should_Filter_By_Search()
    {
        var alpha = CreatePortfolio("Alpha Fund", PortfolioStatus.Active);
        var beta = CreatePortfolio("Beta Fund", PortfolioStatus.Active);
        _portfolioRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Portfolio> { alpha, beta });
        _snapshotRepo.Setup(r => r.GetByPortfolioIdAsync(It.IsAny<Guid>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(new List<PerformanceSnapshot>());
        _tradeRepo.Setup(r => r.GetByPortfolioIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<Domain.Execution.Aggregates.Trade>());

        var service = CreateService();

        var result = await service.GetAllPortfoliosAsync(new PortfolioFilter(Search: "Alpha"));

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Alpha Fund");
    }

    [Fact]
    public async Task GetAllPortfoliosAsync_Should_Filter_By_Search_In_Description()
    {
        var p = CreatePortfolio("Fund", PortfolioStatus.Active);
        p.UpdateDetails("Fund", "A momentum strategy", 0.1m, 1.0m);
        _portfolioRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Portfolio> { p });
        _snapshotRepo.Setup(r => r.GetByPortfolioIdAsync(It.IsAny<Guid>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(new List<PerformanceSnapshot>());
        _tradeRepo.Setup(r => r.GetByPortfolioIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<Domain.Execution.Aggregates.Trade>());

        var service = CreateService();

        var result = await service.GetAllPortfoliosAsync(new PortfolioFilter(Search: "momentum"));

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllPortfoliosAsync_Should_Compute_KPIs_From_Snapshots()
    {
        var portfolio = CreatePortfolio("Test Fund", PortfolioStatus.Active);
        _portfolioRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Portfolio> { portfolio });

        var snapshots = new List<PerformanceSnapshot>
        {
            CreateSnapshot(portfolio.Id, date: new DateOnly(2024, 1, 1), actualReturn: 0.05m, supposedReturn: 0.03m, sharpe: 1.5m, drawdown: -0.02m, equity: 100_000m),
            CreateSnapshot(portfolio.Id, date: new DateOnly(2024, 1, 2), actualReturn: 0.08m, supposedReturn: 0.06m, sharpe: 1.8m, drawdown: -0.05m, equity: 102_000m),
        };
        _snapshotRepo.Setup(r => r.GetByPortfolioIdAsync(portfolio.Id, It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(snapshots);
        _tradeRepo.Setup(r => r.GetByPortfolioIdAsync(portfolio.Id))
            .ReturnsAsync(new List<Domain.Execution.Aggregates.Trade>());

        var service = CreateService();

        var result = await service.GetAllPortfoliosAsync(new PortfolioFilter());

        result.Should().HaveCount(1);
        result[0].ActualReturn.Should().Be(0.08m);
        result[0].SupposedReturn.Should().Be(0.06m);
        result[0].Variance.Should().Be(0.02m);
        result[0].Sharpe.Should().Be(1.8m);
        result[0].MaxDrawdown.Should().Be(-0.05m);
    }

    [Fact]
    public async Task GetAllPortfoliosAsync_Should_Compute_WinRate_From_Trades()
    {
        var portfolio = CreatePortfolio("Fund", PortfolioStatus.Active);
        _portfolioRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Portfolio> { portfolio });
        _snapshotRepo.Setup(r => r.GetByPortfolioIdAsync(It.IsAny<Guid>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(new List<PerformanceSnapshot>());

        var strategyId = Guid.NewGuid();
        var trades = new List<Domain.Execution.Aggregates.Trade>
        {
            CreateClosedTrade(strategyId, portfolio.Id, "BTC", TradeSide.Long, 100m, 110m, 1m),
            CreateClosedTrade(strategyId, portfolio.Id, "ETH", TradeSide.Long, 200m, 190m, 1m),
            CreateClosedTrade(strategyId, portfolio.Id, "SOL", TradeSide.Long, 50m, 60m, 1m),
        };
        _tradeRepo.Setup(r => r.GetByPortfolioIdAsync(portfolio.Id))
            .ReturnsAsync(trades);

        var service = CreateService();

        var result = await service.GetAllPortfoliosAsync(new PortfolioFilter());

        result.Should().HaveCount(1);
        result[0].TradeCount.Should().Be(3);
        result[0].WinRate.Should().Be(2m / 3m);
    }

    [Fact]
    public async Task GetAllPortfoliosAsync_Should_Sort_By_Name_Ascending()
    {
        var z = CreatePortfolio("Zebra Fund", PortfolioStatus.Active);
        var a = CreatePortfolio("Alpha Fund", PortfolioStatus.Active);
        _portfolioRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Portfolio> { z, a });
        _snapshotRepo.Setup(r => r.GetByPortfolioIdAsync(It.IsAny<Guid>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(new List<PerformanceSnapshot>());
        _tradeRepo.Setup(r => r.GetByPortfolioIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<Domain.Execution.Aggregates.Trade>());

        var service = CreateService();

        var result = await service.GetAllPortfoliosAsync(new PortfolioFilter(SortBy: PortfolioSortBy.Name, Ascending: true));

        result[0].Name.Should().Be("Alpha Fund");
        result[1].Name.Should().Be("Zebra Fund");
    }

    [Fact]
    public async Task GetAllPortfoliosAsync_Should_Sort_By_Name_Descending()
    {
        var z = CreatePortfolio("Zebra Fund", PortfolioStatus.Active);
        var a = CreatePortfolio("Alpha Fund", PortfolioStatus.Active);
        _portfolioRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Portfolio> { a, z });
        _snapshotRepo.Setup(r => r.GetByPortfolioIdAsync(It.IsAny<Guid>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(new List<PerformanceSnapshot>());
        _tradeRepo.Setup(r => r.GetByPortfolioIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<Domain.Execution.Aggregates.Trade>());

        var service = CreateService();

        var result = await service.GetAllPortfoliosAsync(new PortfolioFilter(SortBy: PortfolioSortBy.Name, Ascending: false));

        result[0].Name.Should().Be("Zebra Fund");
        result[1].Name.Should().Be("Alpha Fund");
    }

    [Fact]
    public async Task GetAllPortfoliosAsync_Should_Sort_By_ActualReturn()
    {
        var low = CreatePortfolio("Low Return", PortfolioStatus.Active);
        var high = CreatePortfolio("High Return", PortfolioStatus.Active);
        _portfolioRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Portfolio> { low, high });

        _snapshotRepo.Setup(r => r.GetByPortfolioIdAsync(low.Id, It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(new List<PerformanceSnapshot> { CreateSnapshot(low.Id, actualReturn: 0.05m) });
        _snapshotRepo.Setup(r => r.GetByPortfolioIdAsync(high.Id, It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(new List<PerformanceSnapshot> { CreateSnapshot(high.Id, actualReturn: 0.20m) });
        _tradeRepo.Setup(r => r.GetByPortfolioIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<Domain.Execution.Aggregates.Trade>());

        var service = CreateService();

        var result = await service.GetAllPortfoliosAsync(new PortfolioFilter(SortBy: PortfolioSortBy.ActualReturn, Ascending: true));

        result[0].Name.Should().Be("Low Return");
        result[1].Name.Should().Be("High Return");
    }

    [Fact]
    public async Task GetAllPortfoliosAsync_Should_Sort_By_Sharpe()
    {
        var low = CreatePortfolio("Low Sharpe", PortfolioStatus.Active);
        var high = CreatePortfolio("High Sharpe", PortfolioStatus.Active);
        _portfolioRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Portfolio> { low, high });

        _snapshotRepo.Setup(r => r.GetByPortfolioIdAsync(low.Id, It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(new List<PerformanceSnapshot> { CreateSnapshot(low.Id, sharpe: 0.5m) });
        _snapshotRepo.Setup(r => r.GetByPortfolioIdAsync(high.Id, It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(new List<PerformanceSnapshot> { CreateSnapshot(high.Id, sharpe: 2.5m) });
        _tradeRepo.Setup(r => r.GetByPortfolioIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<Domain.Execution.Aggregates.Trade>());

        var service = CreateService();

        var result = await service.GetAllPortfoliosAsync(new PortfolioFilter(SortBy: PortfolioSortBy.Sharpe, Ascending: false));

        result[0].Name.Should().Be("High Sharpe");
        result[1].Name.Should().Be("Low Sharpe");
    }

    [Fact]
    public async Task GetPortfolioDetailAsync_Should_Return_Null_When_Portfolio_Not_Found()
    {
        _portfolioRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Portfolio?)null);
        var service = CreateService();

        var result = await service.GetPortfolioDetailAsync(Guid.NewGuid(), DateRange.All);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPortfolioDetailAsync_Should_Return_Full_Detail()
    {
        var portfolio = CreatePortfolio("Test Fund", PortfolioStatus.Active);
        _portfolioRepo.Setup(r => r.GetByIdAsync(portfolio.Id)).ReturnsAsync(portfolio);

        var strategyId = Guid.NewGuid();
        var strategy = CreateStrategyDefinition(strategyId, "Momentum Strategy", portfolio.Id);
        _strategyRepo.Setup(r => r.GetByPortfolioIdAsync(portfolio.Id))
            .ReturnsAsync(new List<StrategyDefinition> { strategy });

        var strategySnapshots = new List<PerformanceSnapshot>
        {
            CreateSnapshot(portfolio.Id, strategyId: strategyId, date: new DateOnly(2024, 1, 1), actualReturn: 0.1m, supposedReturn: 0.08m, sharpe: 1.2m, equity: 50_000m, drawdown: -0.01m)
        };
        _snapshotRepo.Setup(r => r.GetByStrategyIdAsync(strategyId, It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(strategySnapshots);

        var portfolioSnapshots = new List<PerformanceSnapshot>
        {
            CreateSnapshot(portfolio.Id, date: new DateOnly(2024, 1, 1), actualReturn: 0.1m, supposedReturn: 0.08m, sharpe: 1.2m, equity: 100_000m, drawdown: -0.01m),
            CreateSnapshot(portfolio.Id, date: new DateOnly(2024, 1, 2), actualReturn: 0.15m, supposedReturn: 0.12m, sharpe: 1.5m, equity: 105_000m, drawdown: -0.03m, dailyPnL: 5_000m),
        };
        _snapshotRepo.Setup(r => r.GetByPortfolioIdAsync(portfolio.Id, It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(portfolioSnapshots);

        var trades = new List<Domain.Execution.Aggregates.Trade>
        {
            CreateClosedTrade(strategyId, portfolio.Id, "BTC", TradeSide.Long, 100m, 110m, 1m),
        };
        _tradeRepo.Setup(r => r.GetByPortfolioIdAsync(portfolio.Id))
            .ReturnsAsync(trades);
        _tradeRepo.Setup(r => r.GetByStrategyIdAsync(strategyId))
            .ReturnsAsync(trades);

        var service = CreateService();

        var result = await service.GetPortfolioDetailAsync(portfolio.Id, DateRange.All);

        result.Should().NotBeNull();
        result!.Summary.Should().NotBeNull();
        result.Summary.Name.Should().Be("Test Fund");
        result.Summary.ActualReturn.Should().Be(0.15m);
        result.Strategies.Should().HaveCount(1);
        result.EquityCurve.Should().HaveCount(2);
        result.DrawdownCurve.Should().HaveCount(2);
        result.MonthlyReturns.Should().HaveCount(1);
        result.MonthlyReturns[0].Year.Should().Be(2024);
        result.MonthlyReturns[0].Month.Should().Be(1);
    }

    [Fact]
    public async Task GetPortfolioDetailAsync_Should_Handle_Empty_Snapshots()
    {
        var portfolio = CreatePortfolio("Empty Fund", PortfolioStatus.Active);
        _portfolioRepo.Setup(r => r.GetByIdAsync(portfolio.Id)).ReturnsAsync(portfolio);
        _strategyRepo.Setup(r => r.GetByPortfolioIdAsync(portfolio.Id))
            .ReturnsAsync(new List<StrategyDefinition>());
        _snapshotRepo.Setup(r => r.GetByPortfolioIdAsync(portfolio.Id, It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(new List<PerformanceSnapshot>());
        _tradeRepo.Setup(r => r.GetByPortfolioIdAsync(portfolio.Id))
            .ReturnsAsync(new List<Domain.Execution.Aggregates.Trade>());

        var service = CreateService();

        var result = await service.GetPortfolioDetailAsync(portfolio.Id, DateRange.All);

        result.Should().NotBeNull();
        result!.Summary.ActualReturn.Should().Be(0m);
        result.Summary.SupposedReturn.Should().Be(0m);
        result.Summary.Variance.Should().Be(0m);
        result.Summary.Sharpe.Should().Be(0m);
        result.Summary.MaxDrawdown.Should().Be(0m);
        result.EquityCurve.Should().BeEmpty();
        result.MonthlyReturns.Should().BeEmpty();
    }

    [Fact]
    public async Task GetDashboardKpisAsync_Should_Return_Zero_KPIs_When_No_Data()
    {
        _portfolioRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Portfolio>());
        var service = CreateService();

        var result = await service.GetDashboardKpisAsync(DateRange.All);

        result.TotalAum.Should().Be(0m);
        result.TotalPnl.Should().Be(0m);
        result.AvgSharpe.Should().Be(0m);
        result.MaxDrawdown.Should().Be(0m);
        result.WinRate.Should().Be(0m);
        result.ActiveCount.Should().Be(0);
    }

    [Fact]
    public async Task GetDashboardKpisAsync_Should_Compute_Aggregate_KPIs()
    {
        var p1 = CreatePortfolio("Fund 1", PortfolioStatus.Active, 1_000_000m);
        var p2 = CreatePortfolio("Fund 2", PortfolioStatus.Paused, 2_000_000m);
        _portfolioRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Portfolio> { p1, p2 });

        _snapshotRepo.Setup(r => r.GetByPortfolioIdAsync(p1.Id, It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(new List<PerformanceSnapshot>
            {
                CreateSnapshot(p1.Id, equity: 1_000_000m, dailyPnL: 5_000m, sharpe: 1.5m, drawdown: -0.03m)
            });
        _snapshotRepo.Setup(r => r.GetByPortfolioIdAsync(p2.Id, It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(new List<PerformanceSnapshot>
            {
                CreateSnapshot(p2.Id, equity: 2_000_000m, dailyPnL: 10_000m, sharpe: 2.0m, drawdown: -0.05m)
            });

        var strategyId = Guid.NewGuid();
        var trades = new List<Domain.Execution.Aggregates.Trade>
        {
            CreateClosedTrade(strategyId, p1.Id, "BTC", TradeSide.Long, 100m, 110m, 1m),
            CreateClosedTrade(strategyId, p1.Id, "ETH", TradeSide.Long, 200m, 190m, 1m),
            CreateClosedTrade(strategyId, p2.Id, "SOL", TradeSide.Long, 50m, 60m, 1m),
        };
        _tradeRepo.Setup(r => r.GetByPortfolioIdAsync(p1.Id))
            .ReturnsAsync(trades.Where(t => t.PortfolioId == p1.Id).ToList());
        _tradeRepo.Setup(r => r.GetByPortfolioIdAsync(p2.Id))
            .ReturnsAsync(trades.Where(t => t.PortfolioId == p2.Id).ToList());

        var service = CreateService();

        var result = await service.GetDashboardKpisAsync(DateRange.All);

        result.TotalAum.Should().Be(3_000_000m);
        result.TotalPnl.Should().Be(15_000m);
        result.AvgSharpe.Should().Be(1.75m);
        result.MaxDrawdown.Should().Be(-0.05m);
        result.WinRate.Should().Be(2m / 3m);
        result.ActiveCount.Should().Be(1);
    }

    [Fact]
    public async Task GetPerformanceHistoryAsync_Should_Return_Ordered_Snapshots()
    {
        var portfolioId = Guid.NewGuid();
        var snapshots = new List<PerformanceSnapshot>
        {
            CreateSnapshot(portfolioId, date: new DateOnly(2024, 1, 3), actualReturn: 0.3m, equity: 103_000m),
            CreateSnapshot(portfolioId, date: new DateOnly(2024, 1, 1), actualReturn: 0.1m, equity: 101_000m),
            CreateSnapshot(portfolioId, date: new DateOnly(2024, 1, 2), actualReturn: 0.2m, equity: 102_000m),
        };
        _snapshotRepo.Setup(r => r.GetByPortfolioIdAsync(portfolioId, It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(snapshots);

        var service = CreateService();

        var result = await service.GetPerformanceHistoryAsync(portfolioId, DateRange.All);

        result.Should().HaveCount(3);
        result[0].Date.Should().Be(new DateOnly(2024, 1, 1));
        result[1].Date.Should().Be(new DateOnly(2024, 1, 2));
        result[2].Date.Should().Be(new DateOnly(2024, 1, 3));
        result[0].DailyPnl.Should().Be(0m);
    }

    [Fact]
    public async Task GetPerformanceHistoryAsync_Should_Return_Empty_When_No_Snapshots()
    {
        _snapshotRepo.Setup(r => r.GetByPortfolioIdAsync(It.IsAny<Guid>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(new List<PerformanceSnapshot>());

        var service = CreateService();

        var result = await service.GetPerformanceHistoryAsync(Guid.NewGuid(), DateRange.All);

        result.Should().BeEmpty();
    }

    private PortfolioService CreateService()
    {
        return new PortfolioService(
            _portfolioRepo.Object,
            _strategyRepo.Object,
            _tradeRepo.Object,
            _snapshotRepo.Object);
    }

    private static Portfolio CreatePortfolio(string name, PortfolioStatus status = PortfolioStatus.Active, decimal capital = 1_000_000m)
    {
        var p = new Portfolio(Guid.NewGuid(), name, 0.15m, 1.5m, new Money(capital, Currency.USD));
        if (status == PortfolioStatus.Closed)
            p.Close();
        else if (status == PortfolioStatus.Paused)
            p.Pause();
        return p;
    }

    private static StrategyDefinition CreateStrategyDefinition(Guid strategyId, string name, Guid portfolioId, decimal weight = 0.5m)
    {
        var strategy = new StrategyDefinition(
            strategyId, name, new StrategyType(StrategyCategory.Momentum), 0.15m);
        strategy.AssignToPortfolio(portfolioId, new Weight(weight));
        return strategy;
    }

    private static PerformanceSnapshot CreateSnapshot(
        Guid portfolioId,
        Guid? strategyId = null,
        DateOnly? date = null,
        decimal actualReturn = 0m,
        decimal supposedReturn = 0m,
        decimal sharpe = 0m,
        decimal drawdown = 0m,
        decimal equity = 0m,
        decimal dailyPnL = 0m)
    {
        return new PerformanceSnapshot(
            Guid.NewGuid(),
            Guid.NewGuid(),
            portfolioId,
            strategyId,
            date ?? new DateOnly(2024, 1, 1),
            actualReturn,
            supposedReturn,
            dailyPnL,
            equity,
            drawdown,
            sharpe);
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
