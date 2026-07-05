using Certus.Domain.Market.Enums;
using FluentAssertions;

namespace Certus.Domain.Tests.Market;

public class EnumTests
{
    // --- IndicatorType ---

    [Fact]
    public void IndicatorType_Should_Have_Expected_Members()
    {
        var values = Enum.GetValues<IndicatorType>().Cast<IndicatorType>().ToList();
        values.Should().Contain(new[]
        {
            IndicatorType.RSI,
            IndicatorType.MACD,
            IndicatorType.BollingerBands,
            IndicatorType.ATR,
            IndicatorType.OBV,
            IndicatorType.EMA,
            IndicatorType.SMA,
            IndicatorType.Stochastic,
            IndicatorType.Custom
        });
    }

    [Fact]
    public void IndicatorType_Should_Have_Correct_Values()
    {
        ((int)IndicatorType.RSI).Should().Be(0);
        ((int)IndicatorType.MACD).Should().Be(1);
        ((int)IndicatorType.BollingerBands).Should().Be(2);
        ((int)IndicatorType.ATR).Should().Be(3);
        ((int)IndicatorType.OBV).Should().Be(4);
        ((int)IndicatorType.EMA).Should().Be(5);
        ((int)IndicatorType.SMA).Should().Be(6);
        ((int)IndicatorType.Stochastic).Should().Be(7);
        ((int)IndicatorType.Custom).Should().Be(99);
    }

    [Fact]
    public void IndicatorType_Should_Have_9_Members()
    {
        Enum.GetValues<IndicatorType>().Length.Should().Be(9);
    }

    // --- SignalDirection ---

    [Fact]
    public void SignalDirection_Should_Have_Expected_Members()
    {
        var values = Enum.GetValues<SignalDirection>().Cast<SignalDirection>().ToList();
        values.Should().Contain(new[]
        {
            SignalDirection.Bullish,
            SignalDirection.Bearish,
            SignalDirection.Neutral
        });
    }

    [Fact]
    public void SignalDirection_Should_Have_Correct_Values()
    {
        ((int)SignalDirection.Bullish).Should().Be(0);
        ((int)SignalDirection.Bearish).Should().Be(1);
        ((int)SignalDirection.Neutral).Should().Be(2);
    }

    [Fact]
    public void SignalDirection_Should_Have_3_Members()
    {
        Enum.GetValues<SignalDirection>().Length.Should().Be(3);
    }

    // --- FundamentalFactorType ---

    [Fact]
    public void FundamentalFactorType_Should_Have_Expected_Members()
    {
        var values = Enum.GetValues<FundamentalFactorType>().Cast<FundamentalFactorType>().ToList();
        values.Should().Contain(new[]
        {
            FundamentalFactorType.FundingRate,
            FundamentalFactorType.OpenInterest,
            FundamentalFactorType.SocialSentiment,
            FundamentalFactorType.OnChainMetric,
            FundamentalFactorType.EconomicIndicator,
            FundamentalFactorType.EarningsReport
        });
    }

    [Fact]
    public void FundamentalFactorType_Should_Have_Correct_Values()
    {
        ((int)FundamentalFactorType.FundingRate).Should().Be(0);
        ((int)FundamentalFactorType.OpenInterest).Should().Be(1);
        ((int)FundamentalFactorType.SocialSentiment).Should().Be(2);
        ((int)FundamentalFactorType.OnChainMetric).Should().Be(3);
        ((int)FundamentalFactorType.EconomicIndicator).Should().Be(4);
        ((int)FundamentalFactorType.EarningsReport).Should().Be(5);
    }

    [Fact]
    public void FundamentalFactorType_Should_Have_6_Members()
    {
        Enum.GetValues<FundamentalFactorType>().Length.Should().Be(6);
    }
}
