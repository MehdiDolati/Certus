using Certus.Domain.RiskAndPortfolio.ValueObjects;
using FluentAssertions;

namespace Certus.Domain.Tests.RiskAndPortfolio;

public class DrawdownLimitTests
{
    [Fact]
    public void Constructor_Should_Set_All_Properties()
    {
        var limit = new DrawdownLimit(0.3m, 0.2m, 0.1m);

        limit.MaxDrawdown.Should().Be(0.3m);
        limit.WarningLevel.Should().Be(0.2m);
        limit.HaltLevel.Should().Be(0.1m);
    }

    [Fact]
    public void Constructor_InvalidOrder_HaltAboveWarning_Should_Throw()
    {
        var act = () => new DrawdownLimit(0.3m, 0.1m, 0.2m);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Levels must be ordered*");
    }

    [Fact]
    public void Constructor_InvalidOrder_WarningAboveMax_Should_Throw()
    {
        var act = () => new DrawdownLimit(0.1m, 0.2m, 0.05m);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Levels must be ordered*");
    }

    [Fact]
    public void ShouldHalt_AtHaltLevel_Should_Return_True()
    {
        var limit = new DrawdownLimit(0.3m, 0.2m, 0.1m);

        limit.ShouldHalt(0.1m).Should().BeTrue();
    }

    [Fact]
    public void ShouldHalt_AboveHaltLevel_Should_Return_True()
    {
        var limit = new DrawdownLimit(0.3m, 0.2m, 0.1m);

        limit.ShouldHalt(0.25m).Should().BeTrue();
    }

    [Fact]
    public void ShouldHalt_BelowHaltLevel_Should_Return_False()
    {
        var limit = new DrawdownLimit(0.3m, 0.2m, 0.1m);

        limit.ShouldHalt(0.05m).Should().BeFalse();
    }

    [Fact]
    public void ShouldWarn_AtWarningLevel_Should_Return_True()
    {
        var limit = new DrawdownLimit(0.3m, 0.2m, 0.1m);

        limit.ShouldWarn(0.2m).Should().BeTrue();
    }

    [Fact]
    public void ShouldWarn_AboveWarningLevel_Should_Return_True()
    {
        var limit = new DrawdownLimit(0.3m, 0.2m, 0.1m);

        limit.ShouldWarn(0.25m).Should().BeTrue();
    }

    [Fact]
    public void ShouldWarn_BelowWarningLevel_Should_Return_False()
    {
        var limit = new DrawdownLimit(0.3m, 0.2m, 0.1m);

        limit.ShouldWarn(0.05m).Should().BeFalse();
    }

    [Fact]
    public void Record_Equality_SameValues_Should_Be_Equal()
    {
        var a = new DrawdownLimit(0.3m, 0.2m, 0.1m);
        var b = new DrawdownLimit(0.3m, 0.2m, 0.1m);

        a.Should().Be(b);
    }

    [Fact]
    public void Record_Equality_DifferentValues_Should_Not_Be_Equal()
    {
        var a = new DrawdownLimit(0.3m, 0.2m, 0.1m);
        var b = new DrawdownLimit(0.4m, 0.2m, 0.1m);

        a.Should().NotBe(b);
    }
}
