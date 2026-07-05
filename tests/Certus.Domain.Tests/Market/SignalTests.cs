using Certus.Domain.Market.Aggregates;
using Certus.Domain.Market.Enums;
using Certus.Domain.Market.ValueObjects;
using Certus.Domain.SharedKernel;
using FluentAssertions;

namespace Certus.Domain.Tests.Market;

public class SignalTests
{
    private static readonly Symbol BtcUsd = new("BTC/USD", AssetClass.Crypto);

    [Fact]
    public void Constructor_Should_Set_All_Properties()
    {
        var id = Guid.NewGuid();
        var strength = new SignalStrength(0.8m, 0.9m);
        var expiresAt = DateTime.UtcNow.AddHours(1);

        var signal = new Signal(id, BtcUsd, SignalDirection.Bullish, strength,
            IndicatorType.RSI, "Oversold bounce", expiresAt);

        signal.Id.Should().Be(id);
        signal.Symbol.Should().Be(BtcUsd);
        signal.Direction.Should().Be(SignalDirection.Bullish);
        signal.Strength.Should().Be(strength);
        signal.IndicatorType.Should().Be(IndicatorType.RSI);
        signal.Reason.Should().Be("Oversold bounce");
        signal.GeneratedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        signal.ExpiresAt.Should().Be(expiresAt);
    }

    [Fact]
    public void Constructor_Should_Allow_Null_ExpiresAt()
    {
        var signal = new Signal(Guid.NewGuid(), BtcUsd, SignalDirection.Bearish,
            new SignalStrength(0.5m, 0.5m), IndicatorType.MACD, "Death cross");

        signal.ExpiresAt.Should().BeNull();
    }

    [Fact]
    public void IsExpired_Should_Be_False_When_ExpiresAt_Is_Null()
    {
        var signal = new Signal(Guid.NewGuid(), BtcUsd, SignalDirection.Bullish,
            new SignalStrength(0.5m, 0.5m), IndicatorType.RSI, "reason");

        signal.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_Should_Be_False_When_ExpiresAt_In_Future()
    {
        var signal = new Signal(Guid.NewGuid(), BtcUsd, SignalDirection.Bullish,
            new SignalStrength(0.5m, 0.5m), IndicatorType.RSI, "reason",
            DateTime.UtcNow.AddHours(1));

        signal.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_Should_Be_True_When_ExpiresAt_In_Past()
    {
        var signal = new Signal(Guid.NewGuid(), BtcUsd, SignalDirection.Bullish,
            new SignalStrength(0.5m, 0.5m), IndicatorType.RSI, "reason",
            DateTime.UtcNow.AddHours(-1));

        signal.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_Should_Be_True_When_ExpiresAt_Is_Now()
    {
        var signal = new Signal(Guid.NewGuid(), BtcUsd, SignalDirection.Bullish,
            new SignalStrength(0.5m, 0.5m), IndicatorType.RSI, "reason",
            DateTime.UtcNow.AddMilliseconds(-1));

        signal.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void Constructor_Should_Default_GeneratedAt_To_UtcNow()
    {
        var before = DateTime.UtcNow;
        var signal = new Signal(Guid.NewGuid(), BtcUsd, SignalDirection.Neutral,
            new SignalStrength(0.5m, 0.5m), IndicatorType.SMA, "test");
        var after = DateTime.UtcNow;

        signal.GeneratedAt.Should().BeOnOrAfter(before);
        signal.GeneratedAt.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void Signal_Should_Inherit_Entity_Equality_By_Id()
    {
        var id = Guid.NewGuid();
        var signal1 = new Signal(id, BtcUsd, SignalDirection.Bullish,
            new SignalStrength(0.8m, 0.9m), IndicatorType.RSI, "reason1");
        var signal2 = new Signal(id, BtcUsd, SignalDirection.Bearish,
            new SignalStrength(0.2m, 0.3m), IndicatorType.MACD, "reason2");

        signal1.Should().Be(signal2);
        signal1.GetHashCode().Should().Be(signal2.GetHashCode());
    }

    [Fact]
    public void Signal_Should_Be_Inequality_With_Different_Id()
    {
        var signal1 = new Signal(Guid.NewGuid(), BtcUsd, SignalDirection.Bullish,
            new SignalStrength(0.8m, 0.9m), IndicatorType.RSI, "reason");
        var signal2 = new Signal(Guid.NewGuid(), BtcUsd, SignalDirection.Bullish,
            new SignalStrength(0.8m, 0.9m), IndicatorType.RSI, "reason");

        signal1.Should().NotBe(signal2);
    }
}
