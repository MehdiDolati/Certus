using Certus.Domain.Market.Enums;
using Certus.Domain.Market.Events;
using Certus.Domain.Market.ValueObjects;
using Certus.Domain.SharedKernel;
using FluentAssertions;

namespace Certus.Domain.Tests.Market;

public class DomainEventTests
{
    private static readonly Symbol BtcUsd = new("BTC/USD", AssetClass.Crypto);

    // --- MarketDataReceived ---

    [Fact]
    public void MarketDataReceived_Should_Implement_IDomainEvent()
    {
        typeof(MarketDataReceived).Should().Implement<IDomainEvent>();
    }

    [Fact]
    public void MarketDataReceived_Should_Set_Required_Properties()
    {
        var evt = new MarketDataReceived
        {
            Symbol = BtcUsd,
            TimeFrame = TimeFrame.OneHour,
            BarCount = 100
        };

        evt.Symbol.Should().Be(BtcUsd);
        evt.TimeFrame.Should().Be(TimeFrame.OneHour);
        evt.BarCount.Should().Be(100);
    }

    [Fact]
    public void MarketDataReceived_Should_Default_EventId_To_New_Guid()
    {
        var evt = new MarketDataReceived
        {
            Symbol = BtcUsd,
            TimeFrame = TimeFrame.OneHour
        };

        evt.EventId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void MarketDataReceived_Should_Default_OccurredOn_To_UtcNow()
    {
        var before = DateTime.UtcNow;
        var evt = new MarketDataReceived
        {
            Symbol = BtcUsd,
            TimeFrame = TimeFrame.OneHour
        };
        var after = DateTime.UtcNow;

        evt.OccurredOn.Should().BeOnOrAfter(before);
        evt.OccurredOn.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void MarketDataReceived_Should_Default_BarCount_To_Zero()
    {
        var evt = new MarketDataReceived
        {
            Symbol = BtcUsd,
            TimeFrame = TimeFrame.OneHour
        };

        evt.BarCount.Should().Be(0);
    }

    // --- SignalPublished ---

    [Fact]
    public void SignalPublished_Should_Implement_IDomainEvent()
    {
        typeof(SignalPublished).Should().Implement<IDomainEvent>();
    }

    [Fact]
    public void SignalPublished_Should_Set_All_Required_Properties()
    {
        var strength = new SignalStrength(0.8m, 0.9m);
        var signalId = Guid.NewGuid();

        var evt = new SignalPublished
        {
            SignalId = signalId,
            Symbol = BtcUsd,
            Direction = SignalDirection.Bullish,
            Strength = strength,
            IndicatorType = IndicatorType.RSI
        };

        evt.SignalId.Should().Be(signalId);
        evt.Symbol.Should().Be(BtcUsd);
        evt.Direction.Should().Be(SignalDirection.Bullish);
        evt.Strength.Should().Be(strength);
        evt.IndicatorType.Should().Be(IndicatorType.RSI);
    }

    [Fact]
    public void SignalPublished_Should_Default_EventId_To_New_Guid()
    {
        var evt = new SignalPublished
        {
            SignalId = Guid.NewGuid(),
            Symbol = BtcUsd,
            Direction = SignalDirection.Bearish,
            Strength = new SignalStrength(0.5m, 0.5m),
            IndicatorType = IndicatorType.MACD
        };

        evt.EventId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void SignalPublished_Should_Default_OccurredOn_To_UtcNow()
    {
        var before = DateTime.UtcNow;
        var evt = new SignalPublished
        {
            SignalId = Guid.NewGuid(),
            Symbol = BtcUsd,
            Direction = SignalDirection.Neutral,
            Strength = new SignalStrength(0.5m, 0.5m),
            IndicatorType = IndicatorType.SMA
        };
        var after = DateTime.UtcNow;

        evt.OccurredOn.Should().BeOnOrAfter(before);
        evt.OccurredOn.Should().BeOnOrBefore(after);
    }

    // --- AnomalyDetected ---

    [Fact]
    public void AnomalyDetected_Should_Implement_IDomainEvent()
    {
        typeof(AnomalyDetected).Should().Implement<IDomainEvent>();
    }

    [Fact]
    public void AnomalyDetected_Should_Set_All_Required_Properties()
    {
        var evt = new AnomalyDetected
        {
            Symbol = BtcUsd,
            AnomalyType = AnomalyType.PriceSpike,
            Severity = 0.9m,
            Description = "Flash crash detected"
        };

        evt.Symbol.Should().Be(BtcUsd);
        evt.AnomalyType.Should().Be(AnomalyType.PriceSpike);
        evt.Severity.Should().Be(0.9m);
        evt.Description.Should().Be("Flash crash detected");
    }

    [Fact]
    public void AnomalyDetected_Should_Default_EventId_To_New_Guid()
    {
        var evt = new AnomalyDetected
        {
            Symbol = BtcUsd,
            AnomalyType = AnomalyType.VolumeSpike
        };

        evt.EventId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void AnomalyDetected_Should_Default_OccurredOn_To_UtcNow()
    {
        var before = DateTime.UtcNow;
        var evt = new AnomalyDetected
        {
            Symbol = BtcUsd,
            AnomalyType = AnomalyType.SpreadWidening
        };
        var after = DateTime.UtcNow;

        evt.OccurredOn.Should().BeOnOrAfter(before);
        evt.OccurredOn.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void AnomalyDetected_Should_Default_Severity_To_Zero()
    {
        var evt = new AnomalyDetected
        {
            Symbol = BtcUsd,
            AnomalyType = AnomalyType.LiquidityDrop
        };

        evt.Severity.Should().Be(0m);
    }

    [Fact]
    public void AnomalyDetected_Should_Default_Description_To_Empty()
    {
        var evt = new AnomalyDetected
        {
            Symbol = BtcUsd,
            AnomalyType = AnomalyType.CorrelationBreak
        };

        evt.Description.Should().Be(string.Empty);
    }

    // --- AnomalyType enum ---

    [Fact]
    public void AnomalyType_Should_Have_5_Members()
    {
        Enum.GetValues<AnomalyType>().Length.Should().Be(5);
    }

    [Fact]
    public void AnomalyType_Should_Have_Correct_Values()
    {
        ((int)AnomalyType.PriceSpike).Should().Be(0);
        ((int)AnomalyType.VolumeSpike).Should().Be(1);
        ((int)AnomalyType.SpreadWidening).Should().Be(2);
        ((int)AnomalyType.LiquidityDrop).Should().Be(3);
        ((int)AnomalyType.CorrelationBreak).Should().Be(4);
    }

    [Fact]
    public void Record_Equality_For_MarketDataReceived()
    {
        var evt1 = new MarketDataReceived
        {
            Symbol = BtcUsd,
            TimeFrame = TimeFrame.OneHour,
            BarCount = 50
        };
        var evt2 = new MarketDataReceived
        {
            Symbol = BtcUsd,
            TimeFrame = TimeFrame.OneHour,
            BarCount = 50
        };

        // Different EventIds so records are not equal
        evt1.Should().NotBe(evt2);
    }
}
