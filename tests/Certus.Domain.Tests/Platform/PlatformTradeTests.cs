using Certus.Domain.Platform.ValueObjects;
using FluentAssertions;

namespace Certus.Domain.Tests.Platform;

public class PlatformTradeTests
{
    [Fact]
    public void PlatformTrade_Should_Have_Correct_Defaults()
    {
        var trade = new PlatformTrade();

        trade.ExternalId.Should().BeEmpty();
        trade.StrategyExternalId.Should().BeEmpty();
        trade.Symbol.Should().BeEmpty();
        trade.Side.Should().Be(default);
        trade.Volume.Should().Be(0);
        trade.OpenPrice.Should().Be(0);
        trade.ClosePrice.Should().BeNull();
        trade.StopLoss.Should().Be(0);
        trade.TakeProfit.Should().Be(0);
        trade.Profit.Should().Be(0);
        trade.Commission.Should().Be(0);
        trade.Swap.Should().Be(0);
        trade.OpenTime.Should().Be(default);
        trade.CloseTime.Should().BeNull();
        trade.Comment.Should().BeEmpty();
    }

    [Fact]
    public void PlatformTrade_Should_Be_Created_With_Values()
    {
        var openTime = DateTime.UtcNow.AddHours(-1);
        var closeTime = DateTime.UtcNow;

        var trade = new PlatformTrade
        {
            ExternalId = "12345678",
            StrategyExternalId = "EA_Momentum_01",
            Symbol = "EURUSD",
            Side = TradeSide.Buy,
            Volume = 0.10m,
            OpenPrice = 1.0850m,
            ClosePrice = 1.0875m,
            StopLoss = 1.0830m,
            TakeProfit = 1.0900m,
            Profit = 25.00m,
            Commission = -1.50m,
            Swap = -0.25m,
            OpenTime = openTime,
            CloseTime = closeTime,
            Comment = "Momentum signal",
            Timestamp = DateTime.UtcNow
        };

        trade.ExternalId.Should().Be("12345678");
        trade.StrategyExternalId.Should().Be("EA_Momentum_01");
        trade.Symbol.Should().Be("EURUSD");
        trade.Side.Should().Be(TradeSide.Buy);
        trade.Volume.Should().Be(0.10m);
        trade.OpenPrice.Should().Be(1.0850m);
        trade.ClosePrice.Should().Be(1.0875m);
        trade.Profit.Should().Be(25.00m);
        trade.Commission.Should().Be(-1.50m);
        trade.Swap.Should().Be(-0.25m);
        trade.OpenTime.Should().Be(openTime);
        trade.CloseTime.Should().Be(closeTime);
    }

    [Fact]
    public void PlatformTrade_PnL_Should_Be_Profit_Plus_Commission_Plus_Swap()
    {
        var trade = new PlatformTrade
        {
            Profit = 100m,
            Commission = -5m,
            Swap = -2m
        };

        var pnl = trade.Profit + trade.Commission + trade.Swap;
        pnl.Should().Be(93m);
    }

    [Fact]
    public void PlatformTrade_IsOpen_Should_Be_True_When_CloseTime_Is_Null()
    {
        var trade = new PlatformTrade
        {
            OpenTime = DateTime.UtcNow,
            CloseTime = null
        };

        trade.CloseTime.Should().BeNull();
    }

    [Fact]
    public void PlatformTrade_Duration_Should_Be_Calculated_When_Closed()
    {
        var openTime = new DateTime(2026, 7, 5, 9, 0, 0, DateTimeKind.Utc);
        var closeTime = new DateTime(2026, 7, 5, 10, 30, 0, DateTimeKind.Utc);

        var trade = new PlatformTrade
        {
            OpenTime = openTime,
            CloseTime = closeTime
        };

        var duration = trade.CloseTime!.Value - trade.OpenTime;
        duration.Should().Be(TimeSpan.FromHours(1.5));
    }
}
