using Certus.Domain.Execution.Aggregates;
using Certus.Domain.Execution.Entities;
using Certus.Domain.Execution.Enums;
using Certus.Domain.SharedKernel;
using FluentAssertions;

namespace Certus.Domain.Tests;

public class TradeTests
{
    [Fact]
    public void Trade_Should_Be_Created_With_Correct_Defaults()
    {
        var id = Guid.NewGuid();
        var strategyId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var trade = new Trade(id, strategyId, portfolioId,
            new Symbol("BTC/USD", AssetClass.Crypto), TradeSide.Long, 50000m, 1.5m, "signal");

        trade.Id.Should().Be(id);
        trade.StrategyId.Should().Be(strategyId);
        trade.PortfolioId.Should().Be(portfolioId);
        trade.Symbol.Value.Should().Be("BTC/USD");
        trade.Side.Should().Be(TradeSide.Long);
        trade.EntryPrice.Should().Be(50000m);
        trade.Quantity.Should().Be(1.5m);
        trade.AgentReason.Should().Be("signal");
        trade.Status.Should().Be(TradeStatus.Pending);
    }

    [Fact]
    public void Trade_IsOpen_Should_Return_True_When_Status_Is_Open()
    {
        var trade = CreateTrade();
        var order = CreateAndFillOrder(trade.Id, trade.Symbol.Value, trade.Side, 100m, 50000m);
        trade.Open(order);
        trade.IsOpen.Should().BeTrue();
    }

    [Fact]
    public void Trade_IsClosed_Should_Return_True_When_Status_Is_Closed()
    {
        var trade = CreateTrade();
        var entryOrder = CreateAndFillOrder(trade.Id, trade.Symbol.Value, trade.Side, 100m, 50000m);
        trade.Open(entryOrder);
        var exitOrder = CreateAndFillOrder(trade.Id, trade.Symbol.Value, trade.Side, 100m, 51000m);
        trade.Close(exitOrder);
        trade.IsClosed.Should().BeTrue();
    }

    [Fact]
    public void Trade_Duration_Should_Return_Null_When_No_ExitTime()
    {
        var trade = CreateTrade();
        trade.Duration.Should().BeNull();
    }

    [Fact]
    public void Trade_Duration_Should_Return_Value_When_Trade_Is_Closed()
    {
        var trade = CreateTrade();
        var entryOrder = CreateAndFillOrder(trade.Id, trade.Symbol.Value, trade.Side, 100m, 50000m);
        trade.Open(entryOrder);
        Thread.Sleep(50);
        var exitOrder = CreateAndFillOrder(trade.Id, trade.Symbol.Value, trade.Side, 100m, 51000m);
        trade.Close(exitOrder);
        trade.Duration.Should().NotBeNull();
        trade.Duration!.Value.TotalMilliseconds.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Trade_IsWinning_Should_Return_True_When_PnL_Is_Positive()
    {
        var trade = new Trade(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            new Symbol("ETH/USD", AssetClass.Crypto), TradeSide.Long, 100m, 10m);
        var entryOrder = CreateAndFillOrder(trade.Id, "ETH/USD", TradeSide.Long, 10m, 100m);
        trade.Open(entryOrder);
        var exitOrder = CreateAndFillOrder(trade.Id, "ETH/USD", TradeSide.Long, 10m, 130m);
        trade.Close(exitOrder);
        trade.PnL.Should().BePositive();
        trade.IsWinning.Should().BeTrue();
    }

    [Fact]
    public void Trade_IsWinning_Should_Return_False_When_PnL_Is_Negative()
    {
        var trade = new Trade(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            new Symbol("ETH/USD", AssetClass.Crypto), TradeSide.Long, 100m, 10m);
        var entryOrder = CreateAndFillOrder(trade.Id, "ETH/USD", TradeSide.Long, 10m, 100m);
        trade.Open(entryOrder);
        var exitOrder = CreateAndFillOrder(trade.Id, "ETH/USD", TradeSide.Long, 10m, 50m);
        trade.Close(exitOrder);
        trade.PnL.Should().BeNegative();
        trade.IsWinning.Should().BeFalse();
    }

    private static Trade CreateTrade() =>
        new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            new Symbol("BTC/USD", AssetClass.Crypto), TradeSide.Long, 50000m, 1m);

    private static Order CreateAndFillOrder(Guid tradeId, string symbol, TradeSide side, decimal quantity, decimal price)
    {
        var order = new Order(Guid.NewGuid(), tradeId, symbol, side, OrderType.Market, quantity, ExecutionVenue.Simulated);
        order.Fill(quantity, price, 0m);
        return order;
    }
}
