using Certus.Domain.RiskAndPortfolio.Entities;
using Certus.Domain.RiskAndPortfolio.Enums;
using FluentAssertions;

namespace Certus.Domain.Tests.RiskAndPortfolio;

public class RiskLimitTests
{
    private static RiskLimit CreateSut(
        Guid? portfolioId = null,
        RiskLimitType type = RiskLimitType.MaxDrawdown,
        decimal threshold = 0.1m) =>
        new(Guid.NewGuid(), portfolioId ?? Guid.NewGuid(), type, threshold);

    [Fact]
    public void Constructor_Should_Set_Properties_Correctly()
    {
        var id = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();

        var limit = new RiskLimit(id, portfolioId, RiskLimitType.MaxLeverage, 2.0m);

        limit.Id.Should().Be(id);
        limit.PortfolioId.Should().Be(portfolioId);
        limit.Type.Should().Be(RiskLimitType.MaxLeverage);
        limit.Threshold.Should().Be(2.0m);
        limit.CurrentValue.Should().Be(0m);
        limit.IsBreached.Should().BeFalse();
        limit.Level.Should().Be(RiskLevel.Normal);
    }

    [Fact]
    public void UpdateValue_BelowThreshold_Should_NotBeBreached()
    {
        var limit = CreateSut(threshold: 10m);

        limit.UpdateValue(5m);

        limit.CurrentValue.Should().Be(5m);
        limit.IsBreached.Should().BeFalse();
        limit.Level.Should().Be(RiskLevel.Normal);
    }

    [Fact]
    public void UpdateValue_AtThreshold_Should_NotBeBreached()
    {
        // Breach uses >, not >=
        var limit = CreateSut(threshold: 10m);

        limit.UpdateValue(10m);

        limit.IsBreached.Should().BeFalse();
        limit.Level.Should().Be(RiskLevel.Critical);
    }

    [Fact]
    public void UpdateValue_AboveThreshold_Should_BeBreached()
    {
        var limit = CreateSut(threshold: 10m);

        limit.UpdateValue(10.01m);

        limit.IsBreached.Should().BeTrue();
        limit.Level.Should().Be(RiskLevel.Critical);
    }

    [Fact]
    public void UpdateValue_NegativeValue_Should_Use_Absolute_For_Breach()
    {
        var limit = CreateSut(threshold: 5m);

        limit.UpdateValue(-7m);

        limit.IsBreached.Should().BeTrue();
        limit.Level.Should().Be(RiskLevel.Critical);
    }

    [Theory]
    [InlineData(0.5, RiskLevel.Normal)]
    [InlineData(0.6, RiskLevel.Elevated)]
    [InlineData(0.8, RiskLevel.Warning)]
    [InlineData(1.0, RiskLevel.Critical)]
    [InlineData(1.2, RiskLevel.Critical)]
    public void UpdateValue_Ratio_Should_Determine_RiskLevel(decimal ratio, RiskLevel expectedLevel)
    {
        var limit = CreateSut(threshold: 10m);

        limit.UpdateValue(10m * ratio);

        limit.Level.Should().Be(expectedLevel);
    }

    [Fact]
    public void UpdateValue_ZeroThreshold_AnyPositiveValue_Should_BeBreached()
    {
        // abs(val) > abs(0) is true for any nonzero value
        var limit = CreateSut(threshold: 0m);

        limit.UpdateValue(0.01m);

        limit.IsBreached.Should().BeTrue();
        limit.Level.Should().Be(RiskLevel.Normal);
    }

    [Fact]
    public void UpdateValue_ZeroThreshold_ZeroValue_Should_NotBeBreached()
    {
        var limit = CreateSut(threshold: 0m);

        limit.UpdateValue(0m);

        limit.IsBreached.Should().BeFalse();
    }

    [Fact]
    public void UpdateValue_MultipleUpdates_Should_TrackLatest()
    {
        var limit = CreateSut(threshold: 10m);

        limit.UpdateValue(5m);
        limit.UpdateValue(3m);

        limit.CurrentValue.Should().Be(3m);
        limit.IsBreached.Should().BeFalse();
        limit.Level.Should().Be(RiskLevel.Normal);
    }

    [Fact]
    public void UpdateValue_CanTransition_From_Breached_Back_Normal()
    {
        var limit = CreateSut(threshold: 10m);

        limit.UpdateValue(15m);
        limit.UpdateValue(5m);

        limit.IsBreached.Should().BeFalse();
        limit.Level.Should().Be(RiskLevel.Normal);
    }

    [Fact]
    public void Equals_SameId_Should_Be_Equal()
    {
        var id = Guid.NewGuid();
        var a = new RiskLimit(id, Guid.NewGuid(), RiskLimitType.VaR, 1m);
        var b = new RiskLimit(id, Guid.NewGuid(), RiskLimitType.MaxLeverage, 2m);

        a.Should().Be(b);
    }

    [Fact]
    public void Equals_DifferentId_Should_Not_Be_Equal()
    {
        var a = CreateSut();
        var b = CreateSut();

        a.Should().NotBe(b);
    }
}
