using Certus.Domain.Execution.Enums;
using Certus.Domain.Execution.ValueObjects;
using Certus.Domain.SharedKernel;
using FluentAssertions;

namespace Certus.Domain.Tests.Execution;

public class OrderRequestTests
{
    [Fact]
    public void Constructor_Should_Set_All_Properties()
    {
        var symbol = new Symbol("BTC/USD", AssetClass.Crypto);

        var request = new OrderRequest(symbol, TradeSide.Long, OrderType.Limit, 10m, ExecutionVenue.Binance, 50000m);

        request.Symbol.Should().Be(symbol);
        request.Side.Should().Be(TradeSide.Long);
        request.Type.Should().Be(OrderType.Limit);
        request.Quantity.Should().Be(10m);
        request.Venue.Should().Be(ExecutionVenue.Binance);
        request.Price.Should().Be(50000m);
    }

    [Fact]
    public void Constructor_Should_Allow_Null_Price_For_Market_Orders()
    {
        var symbol = new Symbol("ETH/USD", AssetClass.Crypto);

        var request = new OrderRequest(symbol, TradeSide.Short, OrderType.Market, 5m, ExecutionVenue.Coinbase);

        request.Price.Should().BeNull();
    }

    [Fact]
    public void OrderRequest_Should_Be_Equivalent_By_Value()
    {
        var symbol = new Symbol("BTC/USD", AssetClass.Crypto);

        var r1 = new OrderRequest(symbol, TradeSide.Long, OrderType.Market, 10m, ExecutionVenue.Simulated);
        var r2 = new OrderRequest(symbol, TradeSide.Long, OrderType.Market, 10m, ExecutionVenue.Simulated);

        r1.Should().Be(r2);
    }
}
