using Certus.Application.Trading;
using Certus.Application.Trading.DTOs;
using Certus.Domain.SharedKernel;
using Certus.Domain.Strategy.Enums;
using Certus.Domain.Execution.Enums;
using Certus.Domain.Execution.Entities;
using Certus.Domain.Evaluation.Entities;
using FluentAssertions;
using Moq;

namespace Certus.Application.Tests;

public class TradeServiceTests
{
    private readonly Mock<Certus.Domain.Execution.Repositories.ITradeRepository> _tradeRepo = new();
    private readonly Mock<Certus.Domain.Evaluation.Repositories.IPerformanceSnapshotRepository> _snapshotRepo = new();
    private readonly Mock<Certus.Domain.Strategy.Repositories.IStrategyDefinitionRepository> _strategyRepo = new();

    [Fact]
    public async Task GetTradesAsync_Should_Return_Empty_When_No_Trades()
    {
        _tradeRepo.Setup(r => r.GetFilteredAsync(It.IsAny<Guid?>(), It.IsAny<Guid?>(),
            It.IsAny<Symbol?>(), It.IsAny<TradeSide?>(), It.IsAny<TradeStatus?>(),
            It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<string?>(),
            It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new List<Domain.Execution.Aggregates.Trade>());
        _tradeRepo.Setup(r => r.CountFilteredAsync(It.IsAny<Guid?>(), It.IsAny<Guid?>(),
            It.IsAny<Symbol?>(), It.IsAny<TradeSide?>(), It.IsAny<TradeStatus?>(),
            It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<string?>()))
            .ReturnsAsync(0);

        var service = CreateService();

        var result = await service.GetTradesAsync(new TradeFilter());

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetTradesAsync_Should_Return_Trades_With_Correct_Mapping()
    {
        var strategyId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var trade = CreateClosedTrade(strategyId, portfolioId, "BTCUSD", TradeSide.Long, 40000m, 42000m, 0.5m);

        _tradeRepo.Setup(r => r.CountFilteredAsync(It.IsAny<Guid?>(), It.IsAny<Guid?>(),
            It.IsAny<Symbol?>(), It.IsAny<TradeSide?>(), It.IsAny<TradeStatus?>(),
            It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<string?>()))
            .ReturnsAsync(1);
        _tradeRepo.Setup(r => r.GetFilteredAsync(It.IsAny<Guid?>(), It.IsAny<Guid?>(),
            It.IsAny<Symbol?>(), It.IsAny<TradeSide?>(), It.IsAny<TradeStatus?>(),
            It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<string?>(),
            It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new List<Domain.Execution.Aggregates.Trade> { trade });

        var service = CreateService();

        var result = await service.GetTradesAsync(new TradeFilter());

        result.Items.Should().HaveCount(1);
        result.Items[0].Symbol.Should().Be("BTCUSD");
        result.Items[0].Side.Should().Be(TradeSide.Long);
        result.Items[0].Status.Should().Be(TradeStatus.Closed);
        result.Items[0].EntryPrice.Should().Be(40000m);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetTradesAsync_Should_Pass_Pagination_Parameters()
    {
        _tradeRepo.Setup(r => r.CountFilteredAsync(It.IsAny<Guid?>(), It.IsAny<Guid?>(),
            It.IsAny<Symbol?>(), It.IsAny<TradeSide?>(), It.IsAny<TradeStatus?>(),
            It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<string?>()))
            .ReturnsAsync(0);
        _tradeRepo.Setup(r => r.GetFilteredAsync(It.IsAny<Guid?>(), It.IsAny<Guid?>(),
            It.IsAny<Symbol?>(), It.IsAny<TradeSide?>(), It.IsAny<TradeStatus?>(),
            It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<string?>(),
            2, 10))
            .ReturnsAsync(new List<Domain.Execution.Aggregates.Trade>());

        var service = CreateService();

        var result = await service.GetTradesAsync(new TradeFilter(Page: 2, PageSize: 10));

        result.Items.Should().BeEmpty();
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetTradeDetailAsync_Should_Return_Null_When_Trade_Not_Found()
    {
        _tradeRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Domain.Execution.Aggregates.Trade?)null);
        var service = CreateService();

        var result = await service.GetTradeDetailAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetTradeDetailAsync_Should_Return_Detail_When_Found()
    {
        var strategyId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var trade = CreateClosedTrade(strategyId, portfolioId, "ETHUSD", TradeSide.Short, 3000m, 2800m, 2m, "Short on resistance");

        _tradeRepo.Setup(r => r.GetByIdAsync(trade.Id)).ReturnsAsync(trade);
        _tradeRepo.Setup(r => r.GetByPortfolioIdAsync(portfolioId))
            .ReturnsAsync(new List<Domain.Execution.Aggregates.Trade> { trade });
        _tradeRepo.Setup(r => r.GetByStrategyIdAsync(strategyId))
            .ReturnsAsync(new List<Domain.Execution.Aggregates.Trade> { trade });
        var service = CreateService();

        var result = await service.GetTradeDetailAsync(trade.Id);

        result.Should().NotBeNull();
        result!.Trade.Should().NotBeNull();
        result.Trade.Symbol.Should().Be("ETHUSD");
        result.Trade.Side.Should().Be(TradeSide.Short);
        result.Trade.EntryPrice.Should().Be(3000m);
        result.Trade.ExitPrice.Should().Be(2800m);
        result.Trade.Status.Should().Be(TradeStatus.Closed);
        result.AgentReason.Should().Be("Short on resistance");
    }

    [Fact]
    public async Task GetCumulativePnlAsync_Should_Return_Empty_When_No_Snapshots()
    {
        _snapshotRepo.Setup(r => r.GetByPortfolioIdAsync(It.IsAny<Guid>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(new List<PerformanceSnapshot>());

        var service = CreateService();

        var result = await service.GetCumulativePnlAsync(null, null, DateRange.All);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCumulativePnlAsync_Should_Cumulate_With_PortfolioId()
    {
        var portfolioId = Guid.NewGuid();
        var snapshots = new List<PerformanceSnapshot>
        {
            CreateSnapshot(portfolioId, date: new DateOnly(2024, 1, 1), dailyPnL: 100m),
            CreateSnapshot(portfolioId, date: new DateOnly(2024, 1, 2), dailyPnL: 200m),
            CreateSnapshot(portfolioId, date: new DateOnly(2024, 1, 3), dailyPnL: -50m),
        };
        _snapshotRepo.Setup(r => r.GetByPortfolioIdAsync(portfolioId, It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(snapshots);

        var service = CreateService();

        var result = await service.GetCumulativePnlAsync(portfolioId, null, DateRange.All);

        result.Should().HaveCount(3);
        result[0].DailyPnl.Should().Be(100m);
        result[1].DailyPnl.Should().Be(300m);
        result[2].DailyPnl.Should().Be(250m);
    }

    [Fact]
    public async Task GetCumulativePnlAsync_Should_Cumulate_With_StrategyId()
    {
        var strategyId = Guid.NewGuid();
        var snapshots = new List<PerformanceSnapshot>
        {
            CreateSnapshot(Guid.NewGuid(), strategyId, date: new DateOnly(2024, 1, 1), dailyPnL: 500m),
            CreateSnapshot(Guid.NewGuid(), strategyId, date: new DateOnly(2024, 1, 2), dailyPnL: -100m),
        };
        _snapshotRepo.Setup(r => r.GetByStrategyIdAsync(strategyId, It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(snapshots);

        var service = CreateService();

        var result = await service.GetCumulativePnlAsync(null, strategyId, DateRange.All);

        result.Should().HaveCount(2);
        result[0].DailyPnl.Should().Be(500m);
        result[1].DailyPnl.Should().Be(400m);
    }

    [Fact]
    public async Task GetCumulativePnlAsync_Should_Return_Empty_When_Both_Null()
    {
        var service = CreateService();

        var result = await service.GetCumulativePnlAsync(null, null, DateRange.All);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCumulativePnlAsync_Should_Order_By_Date()
    {
        var portfolioId = Guid.NewGuid();
        var snapshots = new List<PerformanceSnapshot>
        {
            CreateSnapshot(portfolioId, date: new DateOnly(2024, 1, 3), dailyPnL: 300m),
            CreateSnapshot(portfolioId, date: new DateOnly(2024, 1, 1), dailyPnL: 100m),
            CreateSnapshot(portfolioId, date: new DateOnly(2024, 1, 2), dailyPnL: 200m),
        };
        _snapshotRepo.Setup(r => r.GetByPortfolioIdAsync(portfolioId, It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(snapshots);

        var service = CreateService();

        var result = await service.GetCumulativePnlAsync(portfolioId, null, DateRange.All);

        result.Should().HaveCount(3);
        result[0].Date.Should().Be(new DateOnly(2024, 1, 1));
        result[1].Date.Should().Be(new DateOnly(2024, 1, 2));
        result[2].Date.Should().Be(new DateOnly(2024, 1, 3));
    }

    private TradeService CreateService()
    {
        return new TradeService(_tradeRepo.Object, _snapshotRepo.Object, _strategyRepo.Object);
    }

    private static Domain.Execution.Aggregates.Trade CreateClosedTrade(
        Guid strategyId, Guid portfolioId, string symbol, TradeSide side,
        decimal entryPrice, decimal exitPrice, decimal quantity, string agentReason = "")
    {
        var tradeId = Guid.NewGuid();
        var sym = new Symbol(symbol, AssetClass.Crypto);
        var trade = new Domain.Execution.Aggregates.Trade(tradeId, strategyId, portfolioId, sym, side, entryPrice, quantity, agentReason);

        var entryOrder = new Domain.Execution.Entities.Order(
            Guid.NewGuid(), tradeId, symbol, side, OrderType.Market, quantity, ExecutionVenue.Simulated);
        entryOrder.Fill(quantity, entryPrice, 0m);
        trade.Open(entryOrder);

        var exitSide = side == TradeSide.Long ? TradeSide.Short : TradeSide.Long;
        var exitOrder = new Domain.Execution.Entities.Order(
            Guid.NewGuid(), tradeId, symbol, exitSide, OrderType.Market, quantity, ExecutionVenue.Simulated);
        exitOrder.Fill(quantity, exitPrice, 0m);
        trade.Close(exitOrder);

        return trade;
    }

    private static PerformanceSnapshot CreateSnapshot(
        Guid portfolioId, Guid? strategyId = null,
        DateOnly? date = null, decimal dailyPnL = 0m)
    {
        return new PerformanceSnapshot(
            Guid.NewGuid(), Guid.NewGuid(), portfolioId, strategyId,
            date ?? new DateOnly(2024, 1, 1),
            0m, 0m, dailyPnL, 0m, 0m, 0m);
    }
}
