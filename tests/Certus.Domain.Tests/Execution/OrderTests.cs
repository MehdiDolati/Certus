using Certus.Domain.Execution.Entities;
using Certus.Domain.Execution.Enums;
using Certus.Domain.SharedKernel;
using FluentAssertions;

namespace Certus.Domain.Tests.Execution;

public class OrderTests
{
    [Fact]
    public void Constructor_Should_Set_All_Properties()
    {
        var id = Guid.NewGuid();
        var tradeId = Guid.NewGuid();
        var symbol = new Symbol("BTC/USD", AssetClass.Crypto);

        var order = new Order(id, tradeId, "BTC/USD", TradeSide.Long, OrderType.Market, 100m, ExecutionVenue.Binance);

        order.Id.Should().Be(id);
        order.TradeId.Should().Be(tradeId);
        order.Symbol.Should().Be("BTC/USD");
        order.Side.Should().Be(TradeSide.Long);
        order.Type.Should().Be(OrderType.Market);
        order.Quantity.Should().Be(100m);
        order.Venue.Should().Be(ExecutionVenue.Binance);
        order.Status.Should().Be(OrderStatus.Pending);
        order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Constructor_Should_Set_LimitPrice_And_StopPrice()
    {
        var order = new Order(Guid.NewGuid(), Guid.NewGuid(), "ETH/USD", TradeSide.Short,
            OrderType.Limit, 10m, ExecutionVenue.Coinbase, limitPrice: 3000m, stopPrice: 2900m);

        order.LimitPrice.Should().Be(3000m);
        order.StopPrice.Should().Be(2900m);
    }

    [Fact]
    public void Submit_Should_Change_Status_To_Submitted()
    {
        var order = CreateOrder();

        order.Submit("EX-123");

        order.Status.Should().Be(OrderStatus.Submitted);
        order.ExchangeOrderId.Should().Be("EX-123");
    }

    [Fact]
    public void Fill_Should_Update_FilledQuantity_And_AveragePrice()
    {
        var order = CreateOrder();

        order.Fill(50m, 50000m, 5m);

        order.FilledQuantity.Should().Be(50m);
        order.AverageFillPrice.Should().Be(50000m);
        order.TotalFees.Should().Be(5m);
        order.Status.Should().Be(OrderStatus.PartiallyFilled);
    }

    [Fact]
    public void Fill_Should_Calculate_Weighted_Average_Price()
    {
        var order = CreateOrder();

        order.Fill(60m, 50000m, 3m);
        order.Fill(40m, 52000m, 2m);

        order.FilledQuantity.Should().Be(100m);
        order.AverageFillPrice.Should().Be(50800m); // (60*50000 + 40*52000) / 100 = 50800
        order.TotalFees.Should().Be(5m);
    }

    [Fact]
    public void Fill_Should_Set_Status_To_Filled_When_Fully_Filled()
    {
        var order = CreateOrder();

        order.Fill(100m, 50000m, 0m);

        order.Status.Should().Be(OrderStatus.Filled);
        order.IsFullyFilled.Should().BeTrue();
        order.FilledAt.Should().NotBeNull();
    }

    [Fact]
    public void Fill_Should_Set_Slippage_For_Limit_Orders()
    {
        var order = new Order(Guid.NewGuid(), Guid.NewGuid(), "BTC/USD", TradeSide.Long,
            OrderType.Limit, 100m, ExecutionVenue.Simulated, limitPrice: 49900m);

        order.Fill(100m, 50000m, 0m);

        // |50000 - 49900| / 49900 * 100 = 100/49900 * 100 = ~0.2004%
        order.Slippage.Should().BeApproximately(0.2004m, 0.001m);
    }

    [Fact]
    public void Fill_Should_Set_Slippage_To_Zero_For_Market_Orders()
    {
        var order = new Order(Guid.NewGuid(), Guid.NewGuid(), "BTC/USD", TradeSide.Long,
            OrderType.Market, 100m, ExecutionVenue.Simulated);

        order.Fill(100m, 50000m, 0m);

        order.Slippage.Should().Be(0m);
    }

    [Fact]
    public void IsFullyFilled_Should_Be_False_When_Partially_Filled()
    {
        var order = CreateOrder();

        order.Fill(50m, 50000m, 0m);

        order.IsFullyFilled.Should().BeFalse();
    }

    [Fact]
    public void FillPercentage_Should_Calculate_Correctly()
    {
        var order = CreateOrder();

        order.Fill(30m, 50000m, 0m);

        order.FillPercentage.Should().Be(30m);
    }

    [Fact]
    public void FillPercentage_Should_Be_Zero_When_Quantity_Is_Zero()
    {
        var order = new Order(Guid.NewGuid(), Guid.NewGuid(), "BTC/USD", TradeSide.Long,
            OrderType.Market, 0m, ExecutionVenue.Simulated);

        order.FillPercentage.Should().Be(0m);
    }

    [Fact]
    public void Cancel_Should_Change_Status_To_Cancelled()
    {
        var order = CreateOrder();

        order.Cancel();

        order.Status.Should().Be(OrderStatus.Cancelled);
        order.CancelledAt.Should().NotBeNull();
    }

    [Fact]
    public void Reject_Should_Change_Status_To_Rejected()
    {
        var order = CreateOrder();

        order.Reject("insufficient funds");

        order.Status.Should().Be(OrderStatus.Rejected);
        order.CancelledAt.Should().NotBeNull();
    }

    private static Order CreateOrder() =>
        new(Guid.NewGuid(), Guid.NewGuid(), "BTC/USD", TradeSide.Long, OrderType.Market, 100m, ExecutionVenue.Simulated);
}
