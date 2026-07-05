using Certus.Domain.RiskAndPortfolio.Enums;
using Certus.Domain.RiskAndPortfolio.ValueObjects;
using FluentAssertions;

namespace Certus.Domain.Tests.RiskAndPortfolio;

public class AdaptiveRiskParamsTests
{
    [Fact]
    public void Constructor_Should_Set_All_Properties()
    {
        var sut = new AdaptiveRiskParams(1.0m, 0.7m, MarketRegime.Volatile, 1.5m);

        sut.VolatilityScalingFactor.Should().Be(1.0m);
        sut.ConfidenceThreshold.Should().Be(0.7m);
        sut.MarketRegime.Should().Be(MarketRegime.Volatile);
        sut.MaxPositionMultiplier.Should().Be(1.5m);
    }

    [Fact]
    public void Constructor_DefaultMaxPositionMultiplier_Should_Be_One()
    {
        var sut = new AdaptiveRiskParams(1.0m, 0.7m, MarketRegime.Normal);

        sut.MaxPositionMultiplier.Should().Be(1.0m);
    }

    [Fact]
    public void Conservative_Should_Have_Correct_Values()
    {
        AdaptiveRiskParams.Conservative.VolatilityScalingFactor.Should().Be(0.5m);
        AdaptiveRiskParams.Conservative.ConfidenceThreshold.Should().Be(0.8m);
        AdaptiveRiskParams.Conservative.MarketRegime.Should().Be(MarketRegime.Normal);
        AdaptiveRiskParams.Conservative.MaxPositionMultiplier.Should().Be(1.0m);
    }

    [Fact]
    public void Moderate_Should_Have_Correct_Values()
    {
        AdaptiveRiskParams.Moderate.VolatilityScalingFactor.Should().Be(1.0m);
        AdaptiveRiskParams.Moderate.ConfidenceThreshold.Should().Be(0.6m);
        AdaptiveRiskParams.Moderate.MarketRegime.Should().Be(MarketRegime.Normal);
    }

    [Fact]
    public void Aggressive_Should_Have_Correct_Values()
    {
        AdaptiveRiskParams.Aggressive.VolatilityScalingFactor.Should().Be(1.5m);
        AdaptiveRiskParams.Aggressive.ConfidenceThreshold.Should().Be(0.4m);
        AdaptiveRiskParams.Aggressive.MarketRegime.Should().Be(MarketRegime.Normal);
    }

    [Fact]
    public void Crisis_Should_Have_Correct_Values()
    {
        AdaptiveRiskParams.Crisis.VolatilityScalingFactor.Should().Be(0.3m);
        AdaptiveRiskParams.Crisis.ConfidenceThreshold.Should().Be(0.9m);
        AdaptiveRiskParams.Crisis.MarketRegime.Should().Be(MarketRegime.Crisis);
        AdaptiveRiskParams.Crisis.MaxPositionMultiplier.Should().Be(0.5m);
    }

    [Fact]
    public void Record_Equality_SameValues_Should_Be_Equal()
    {
        var a = new AdaptiveRiskParams(1.0m, 0.7m, MarketRegime.Normal);
        var b = new AdaptiveRiskParams(1.0m, 0.7m, MarketRegime.Normal);

        a.Should().Be(b);
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void Record_Equality_DifferentValues_Should_Not_Be_Equal()
    {
        var a = new AdaptiveRiskParams(1.0m, 0.7m, MarketRegime.Normal);
        var b = new AdaptiveRiskParams(1.5m, 0.7m, MarketRegime.Normal);

        a.Should().NotBe(b);
        (a != b).Should().BeTrue();
    }
}
