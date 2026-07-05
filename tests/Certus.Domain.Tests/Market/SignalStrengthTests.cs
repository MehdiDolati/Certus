using Certus.Domain.Market.ValueObjects;
using FluentAssertions;

namespace Certus.Domain.Tests.Market;

public class SignalStrengthTests
{
    [Fact]
    public void Constructor_Should_Set_Value_And_Confidence()
    {
        var strength = new SignalStrength(0.8m, 0.9m);

        strength.Value.Should().Be(0.8m);
        strength.Confidence.Should().Be(0.9m);
    }

    [Fact]
    public void Constructor_Should_Throw_When_Value_Below_Zero()
    {
        var act = () => new SignalStrength(-0.1m, 0.5m);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .Which.ParamName.Should().Be("value");
    }

    [Fact]
    public void Constructor_Should_Throw_When_Value_Above_One()
    {
        var act = () => new SignalStrength(1.1m, 0.5m);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .Which.ParamName.Should().Be("value");
    }

    [Fact]
    public void Constructor_Should_Throw_When_Confidence_Below_Zero()
    {
        var act = () => new SignalStrength(0.5m, -0.1m);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .Which.ParamName.Should().Be("confidence");
    }

    [Fact]
    public void Constructor_Should_Throw_When_Confidence_Above_One()
    {
        var act = () => new SignalStrength(0.5m, 1.1m);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .Which.ParamName.Should().Be("confidence");
    }

    [Fact]
    public void Constructor_Should_Accept_Boundary_Values()
    {
        var strength = new SignalStrength(0m, 0m);

        strength.Value.Should().Be(0m);
        strength.Confidence.Should().Be(0m);

        var max = new SignalStrength(1m, 1m);

        max.Value.Should().Be(1m);
        max.Confidence.Should().Be(1m);
    }

    [Fact]
    public void IsStrong_Should_Be_True_When_Value_Gte_07_And_Confidence_Gte_06()
    {
        new SignalStrength(0.7m, 0.6m).IsStrong.Should().BeTrue();
        new SignalStrength(0.8m, 0.9m).IsStrong.Should().BeTrue();
        new SignalStrength(1m, 1m).IsStrong.Should().BeTrue();
    }

    [Fact]
    public void IsStrong_Should_Be_False_When_Value_Below_07()
    {
        new SignalStrength(0.69m, 0.9m).IsStrong.Should().BeFalse();
    }

    [Fact]
    public void IsStrong_Should_Be_False_When_Confidence_Below_06()
    {
        new SignalStrength(0.9m, 0.59m).IsStrong.Should().BeFalse();
    }

    [Fact]
    public void IsWeak_Should_Be_True_When_Value_Below_03()
    {
        new SignalStrength(0.29m, 0.9m).IsWeak.Should().BeTrue();
    }

    [Fact]
    public void IsWeak_Should_Be_True_When_Confidence_Below_04()
    {
        new SignalStrength(0.9m, 0.39m).IsWeak.Should().BeTrue();
    }

    [Fact]
    public void IsWeak_Should_Be_False_When_Both_At_Thresholds()
    {
        new SignalStrength(0.3m, 0.4m).IsWeak.Should().BeFalse();
    }

    [Fact]
    public void Record_Equality_Should_Work()
    {
        var s1 = new SignalStrength(0.5m, 0.6m);
        var s2 = new SignalStrength(0.5m, 0.6m);

        s1.Should().Be(s2);
        s1.GetHashCode().Should().Be(s2.GetHashCode());
    }

    [Fact]
    public void Record_Inequality_Should_Work()
    {
        var s1 = new SignalStrength(0.5m, 0.6m);
        var s2 = new SignalStrength(0.5m, 0.7m);

        s1.Should().NotBe(s2);
    }
}
