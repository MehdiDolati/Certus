using Certus.Domain.Execution.Aggregates;
using Certus.Domain.Execution.Entities;
using Certus.Domain.Execution.Enums;
using Certus.Domain.Execution.Events;
using Certus.Domain.SharedKernel;
using FluentAssertions;

namespace Certus.Domain.Tests.Execution;

public class TradeTests
{
    private static readonly Symbol BtcUsd = new("BTC/USD", AssetClass.Crypto);
    private static readonly Symbol EthUsd = new("ETH/USD", AssetClass.Crypto);

    // --- Constructor ---

    [Fact]
    public void Constructor_Should_Set_All_Properties_Correctly()
    {
        var id = Guid.NewGuid();
        var strategyId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();

        var trade = new Trade(id, strategyId, portfolioId, BtcUsd, TradeSide.Long, 50000m, 1.5m, "signal");

        trade.Id.Should().Be(id);
        trade.StrategyId.Should().Be(strategyId);
        trade.PortfolioId.Should().Be(portfolioId);
        trade.Symbol.Should().Be(BtcUsd);
        trade.Side.Should().Be(TradeSide.Long);
        trade.EntryPrice.Should().Be(50000m);
        trade.Quantity.Should().Be(1.5m);
        trade.AgentReason.Should().Be("signal");
        trade.Status.Should().Be(TradeStatus.Pending);
        trade.EntryTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        trade.PnL.Should().Be(0m);
        trade.PnLPercent.Should().Be(0m);
        trade.Fees.Should().Be(0m);
        trade.Slippage.Should().Be(0m);
        trade.ExitTime.Should().BeNull();
        trade.ExitPrice.Should().BeNull();
    }

    [Fact]
    public void Constructor_Should_Default_AgentReason_To_Empty()
    {
        var trade = new Trade(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            BtcUsd, TradeSide.Long, 50000m, 1m);

        trade.AgentReason.Should().Be(string.Empty);
    }

    [Fact]
    public void Constructor_Should_Raise_TradeOpened_DomainEvent()
    {
        var id = Guid.NewGuid();
        var strategyId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();

        var trade = new Trade(id, strategyId, portfolioId, BtcUsd, TradeSide.Long, 50000m, 1m);

        trade.DomainEvents.Should().HaveCount(1);
        var evt = trade.DomainEvents[0].Should().BeOfType<TradeOpened>().Subject;
        evt.TradeId.Should().Be(id);
        evt.PortfolioId.Should().Be(portfolioId);
        evt.StrategyId.Should().Be(strategyId);
        evt.Symbol.Should().Be(BtcUsd);
        evt.Side.Should().Be(TradeSide.Long);
        evt.Size.Should().Be(1m);
    }

    // --- Status Transitions ---

    [Fact]
    public void Open_Should_Transition_From_Pending_To_Open()
    {
        var trade = CreateTrade();
        var order = CreateFilledOrder(trade.Id, 100m, 50000m);

        trade.Open(order);

        trade.Status.Should().Be(TradeStatus.Open);
    }

    [Fact]
    public void Open_Should_Throw_When_Status_Is_Not_Pending()
    {
        var trade = CreateTrade();
        var order = CreateFilledOrder(trade.Id, 100m, 50000m);
        trade.Open(order);

        var act = () => trade.Open(CreateFilledOrder(trade.Id, 100m, 51000m));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*pending*");
    }

    [Fact]
    public void Close_Should_Transition_From_Open_To_Closed()
    {
        var trade = CreateTrade();
        trade.Open(CreateFilledOrder(trade.Id, 100m, 50000m));

        trade.Close(CreateFilledOrder(trade.Id, 100m, 51000m));

        trade.Status.Should().Be(TradeStatus.Closed);
    }

    [Fact]
    public void Close_Should_Throw_When_Status_Is_Not_Open()
    {
        var trade = CreateTrade();

        var act = () => trade.Close(CreateFilledOrder(trade.Id, 100m, 51000m));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*open*");
    }

    [Fact]
    public void Cancel_From_Pending_Should_Transition_To_Cancelled()
    {
        var trade = CreateTrade();

        trade.Cancel("no signal");

        trade.Status.Should().Be(TradeStatus.Cancelled);
    }

    [Fact]
    public void Cancel_From_Open_Should_Transition_To_Cancelled()
    {
        var trade = CreateTrade();
        trade.Open(CreateFilledOrder(trade.Id, 100m, 50000m));

        trade.Cancel("stop loss hit");

        trade.Status.Should().Be(TradeStatus.Cancelled);
    }

    [Fact]
    public void Cancel_Should_Throw_When_Status_Is_Closed()
    {
        var trade = CreateTrade();
        trade.Open(CreateFilledOrder(trade.Id, 100m, 50000m));
        trade.Close(CreateFilledOrder(trade.Id, 100m, 51000m));

        var act = () => trade.Cancel("too late");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*closed*");
    }

    // --- PnL Calculation ---

    [Fact]
    public void PnL_Should_Be_Correct_For_Long_Trade_Profit()
    {
        var trade = new Trade(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            BtcUsd, TradeSide.Long, 100m, 10m);
        trade.Open(CreateFilledOrder(trade.Id, 10m, 100m));
        trade.Close(CreateFilledOrder(trade.Id, 10m, 130m));

        // (130 - 100) * 10 = 300
        trade.PnL.Should().Be(300m);
    }

    [Fact]
    public void PnL_Should_Be_Correct_For_Long_Trade_Loss()
    {
        var trade = new Trade(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            BtcUsd, TradeSide.Long, 100m, 10m);
        trade.Open(CreateFilledOrder(trade.Id, 10m, 100m));
        trade.Close(CreateFilledOrder(trade.Id, 10m, 50m));

        // (50 - 100) * 10 = -500
        trade.PnL.Should().Be(-500m);
    }

    [Fact]
    public void PnL_Should_Be_Correct_For_Short_Trade_Profit()
    {
        var trade = new Trade(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            BtcUsd, TradeSide.Short, 100m, 10m);
        trade.Open(CreateFilledOrder(trade.Id, 10m, 100m));
        trade.Close(CreateFilledOrder(trade.Id, 10m, 50m));

        // (100 - 50) * 10 = 500
        trade.PnL.Should().Be(500m);
    }

    [Fact]
    public void PnL_Should_Be_Correct_For_Short_Trade_Loss()
    {
        var trade = new Trade(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            BtcUsd, TradeSide.Short, 100m, 10m);
        trade.Open(CreateFilledOrder(trade.Id, 10m, 100m));
        trade.Close(CreateFilledOrder(trade.Id, 10m, 130m));

        // (100 - 130) * 10 = -300
        trade.PnL.Should().Be(-300m);
    }

    [Fact]
    public void PnLPercent_Should_Be_Calculated_Correctly()
    {
        var trade = new Trade(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            BtcUsd, TradeSide.Long, 100m, 10m);
        trade.Open(CreateFilledOrder(trade.Id, 10m, 100m));
        trade.Close(CreateFilledOrder(trade.Id, 10m, 120m));

        // PnL = (120 - 100) * 10 = 200
        // PnLPercent = 200 / (100 * 10) * 100 = 20%
        trade.PnLPercent.Should().Be(20m);
    }

    // --- IsOpen / IsClosed / IsWinning ---

    [Fact]
    public void IsOpen_Should_Be_True_Only_When_Open()
    {
        var trade = CreateTrade();
        trade.IsOpen.Should().BeFalse();

        trade.Open(CreateFilledOrder(trade.Id, 100m, 50000m));
        trade.IsOpen.Should().BeTrue();

        trade.Close(CreateFilledOrder(trade.Id, 100m, 51000m));
        trade.IsOpen.Should().BeFalse();
    }

    [Fact]
    public void IsClosed_Should_Be_True_Only_When_Closed()
    {
        var trade = CreateTrade();
        trade.IsClosed.Should().BeFalse();

        trade.Open(CreateFilledOrder(trade.Id, 100m, 50000m));
        trade.IsClosed.Should().BeFalse();

        trade.Close(CreateFilledOrder(trade.Id, 100m, 51000m));
        trade.IsClosed.Should().BeTrue();
    }

    [Fact]
    public void IsWinning_Should_Be_True_When_PnL_Positive()
    {
        var trade = new Trade(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            EthUsd, TradeSide.Long, 100m, 10m);
        trade.Open(CreateFilledOrder(trade.Id, 10m, 100m));
        trade.Close(CreateFilledOrder(trade.Id, 10m, 130m));

        trade.IsWinning.Should().BeTrue();
    }

    [Fact]
    public void IsWinning_Should_Be_False_When_PnL_Negative()
    {
        var trade = new Trade(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            EthUsd, TradeSide.Long, 100m, 10m);
        trade.Open(CreateFilledOrder(trade.Id, 10m, 100m));
        trade.Close(CreateFilledOrder(trade.Id, 10m, 50m));

        trade.IsWinning.Should().BeFalse();
    }

    // --- Duration ---

    [Fact]
    public void Duration_Should_Be_Null_Before_Closing()
    {
        var trade = CreateTrade();
        trade.Duration.Should().BeNull();
    }

    [Fact]
    public void Duration_Should_Be_NonZero_After_Closing()
    {
        var trade = CreateTrade();
        trade.Open(CreateFilledOrder(trade.Id, 100m, 50000m));
        Thread.Sleep(50);
        trade.Close(CreateFilledOrder(trade.Id, 100m, 51000m));

        trade.Duration.Should().NotBeNull();
        trade.Duration!.Value.TotalMilliseconds.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Duration_Should_Be_NonZero_After_Cancellation()
    {
        var trade = CreateTrade();
        trade.Open(CreateFilledOrder(trade.Id, 100m, 50000m));
        Thread.Sleep(50);
        trade.Cancel("test");

        trade.Duration.Should().NotBeNull();
        trade.Duration!.Value.TotalMilliseconds.Should().BeGreaterThan(0);
    }

    // --- Fees and Slippage ---

    [Fact]
    public void Fees_Should_Accumulate_From_Both_Orders()
    {
        var trade = CreateTrade();
        var entryOrder = CreateFilledOrder(trade.Id, 100m, 50000m);
        entryOrder.Fill(0, 0, 10m); // Add extra fees via second fill call won't work due to logic...

        // Actually fees come from order.TotalFees via Open/Close
        // Let's make an order with fees baked in
        var orderWithFees = new Order(Guid.NewGuid(), trade.Id, "BTC/USD", TradeSide.Long, OrderType.Market, 100m, ExecutionVenue.Simulated);
        orderWithFees.Fill(100m, 50000m, 5m);

        trade.Open(orderWithFees);
        trade.Fees.Should().Be(5m);

        var exitOrder = new Order(Guid.NewGuid(), trade.Id, "BTC/USD", TradeSide.Long, OrderType.Market, 100m, ExecutionVenue.Simulated);
        exitOrder.Fill(100m, 51000m, 8m);
        trade.Close(exitOrder);

        trade.Fees.Should().Be(13m);
    }

    [Fact]
    public void Slippage_Should_Accumulate_From_Both_Orders()
    {
        var trade = CreateTrade();
        var entryOrder = new Order(Guid.NewGuid(), trade.Id, "BTC/USD", TradeSide.Long, OrderType.Limit, 100m, ExecutionVenue.Simulated, limitPrice: 49900m);
        entryOrder.Fill(100m, 50000m, 0m);

        trade.Open(entryOrder);
        trade.Slippage.Should().BeGreaterThan(0);

        var exitOrder = new Order(Guid.NewGuid(), trade.Id, "BTC/USD", TradeSide.Long, OrderType.Limit, 100m, ExecutionVenue.Simulated, limitPrice: 51100m);
        exitOrder.Fill(100m, 51000m, 0m);
        trade.Close(exitOrder);

        trade.Slippage.Should().BeGreaterThan(0);
    }

    // --- Lifecycle ---

    [Fact]
    public void Lifecycle_Should_Be_Updated_On_Open()
    {
        var trade = CreateTrade();
        trade.Lifecycle.SignalTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));

        var order = CreateFilledOrder(trade.Id, 100m, 50000m);
        trade.Open(order);

        trade.Lifecycle.OrderTime.Should().NotBeNull();
        trade.Lifecycle.FillTime.Should().BeNull();
    }

    [Fact]
    public void Lifecycle_Should_Be_Updated_On_Close()
    {
        var trade = CreateTrade();
        trade.Open(CreateFilledOrder(trade.Id, 100m, 50000m));

        trade.Close(CreateFilledOrder(trade.Id, 100m, 51000m));

        trade.Lifecycle.FillTime.Should().NotBeNull();
    }

    [Fact]
    public void Lifecycle_Should_Be_Updated_On_Cancel()
    {
        var trade = CreateTrade();
        trade.Open(CreateFilledOrder(trade.Id, 100m, 50000m));

        trade.Cancel("test");

        trade.Lifecycle.CloseTime.Should().NotBeNull();
    }

    // --- Execution Logs ---

    [Fact]
    public void Open_Should_Add_ExecutionLog()
    {
        var trade = CreateTrade();
        trade.Open(CreateFilledOrder(trade.Id, 100m, 50000m));

        trade.ExecutionLogs.Should().HaveCount(1);
        trade.ExecutionLogs[0].Action.Should().Be("Trade opened");
    }

    [Fact]
    public void Close_Should_Add_ExecutionLog()
    {
        var trade = CreateTrade();
        trade.Open(CreateFilledOrder(trade.Id, 100m, 50000m));
        trade.Close(CreateFilledOrder(trade.Id, 100m, 51000m));

        trade.ExecutionLogs.Should().HaveCount(2);
        trade.ExecutionLogs[1].Action.Should().Be("Trade closed");
    }

    [Fact]
    public void Cancel_Should_Add_ExecutionLog()
    {
        var trade = CreateTrade();
        trade.Cancel("reason");

        trade.ExecutionLogs.Should().HaveCount(1);
        trade.ExecutionLogs[0].Action.Should().Be("Trade cancelled");
        trade.ExecutionLogs[0].Details.Should().Be("reason");
    }

    // --- Domain Events ---

    [Fact]
    public void Close_Should_Raise_TradeClosed_Event()
    {
        var trade = CreateTrade();
        trade.Open(CreateFilledOrder(trade.Id, 100m, 50000m));
        trade.ClearDomainEvents();

        trade.Close(CreateFilledOrder(trade.Id, 100m, 51000m));

        var evt = trade.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TradeClosed>().Subject;
        evt.TradeId.Should().Be(trade.Id);
    }

    [Fact]
    public void Cancel_Should_Raise_TradeCancelled_Event()
    {
        var trade = CreateTrade();
        trade.ClearDomainEvents();

        trade.Cancel("no signal");

        var evt = trade.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TradeCancelled>().Subject;
        evt.TradeId.Should().Be(trade.Id);
        evt.Reason.Should().Be("no signal");
    }

    // --- Orders Collection ---

    [Fact]
    public void Open_Should_Add_Entry_Order_To_Orders()
    {
        var trade = CreateTrade();
        var order = CreateFilledOrder(trade.Id, 100m, 50000m);

        trade.Open(order);

        trade.Orders.Should().HaveCount(1);
        trade.Orders[0].Should().Be(order);
    }

    [Fact]
    public void Close_Should_Add_Exit_Order_To_Orders()
    {
        var trade = CreateTrade();
        trade.Open(CreateFilledOrder(trade.Id, 100m, 50000m));
        var exitOrder = CreateFilledOrder(trade.Id, 100m, 51000m);

        trade.Close(exitOrder);

        trade.Orders.Should().HaveCount(2);
        trade.Orders[1].Should().Be(exitOrder);
    }

    // --- Helpers ---

    private static Trade CreateTrade() =>
        new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            BtcUsd, TradeSide.Long, 50000m, 1m);

    private static Order CreateFilledOrder(Guid tradeId, decimal quantity, decimal price)
    {
        var order = new Order(Guid.NewGuid(), tradeId, "BTC/USD", TradeSide.Long, OrderType.Market, quantity, ExecutionVenue.Simulated);
        order.Fill(quantity, price, 0m);
        return order;
    }
}
