using Certus.Domain.Strategy.ValueObjects;
using FluentAssertions;

namespace Certus.Domain.Tests.Strategy;

public class RiskProfileTests
{
    [Fact]
    public void Constructor_Should_Set_Properties_Correctly()
    {
        var profile = new RiskProfile(0.2m, 0.3m, 0.15m);

        profile.MaxDrawdownLimit.Should().Be(0.2m);
        profile.MaxPositionSize.Should().Be(0.3m);
        profile.VolatilityTarget.Should().Be(0.15m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.5)]
    [InlineData(1)]
    public void Constructor_Valid_MaxDrawdownLimit_Should_Succeed(decimal maxDrawdown)
    {
        var act = () => new RiskProfile(maxDrawdown, 0.3m, 0.15m);

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void Constructor_Invalid_MaxDrawdownLimit_Should_Throw(decimal maxDrawdown)
    {
        var act = () => new RiskProfile(maxDrawdown, 0.3m, 0.15m);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.5)]
    [InlineData(1)]
    public void Constructor_Valid_MaxPositionSize_Should_Succeed(decimal positionSize)
    {
        var act = () => new RiskProfile(0.2m, positionSize, 0.15m);

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void Constructor_Invalid_MaxPositionSize_Should_Throw(decimal positionSize)
    {
        var act = () => new RiskProfile(0.2m, positionSize, 0.15m);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Record_Should_Be_Equal_When_Values_Are_Same()
    {
        var profile1 = new RiskProfile(0.2m, 0.3m, 0.15m);
        var profile2 = new RiskProfile(0.2m, 0.3m, 0.15m);

        profile1.Should().Be(profile2);
    }

    [Fact]
    public void Record_Should_Be_Inequal_When_Values_Differ()
    {
        var profile1 = new RiskProfile(0.2m, 0.3m, 0.15m);
        var profile2 = new RiskProfile(0.2m, 0.4m, 0.15m);

        profile1.Should().NotBe(profile2);
    }
}
