using Certus.Domain.RiskAndPortfolio.ValueObjects;
using FluentAssertions;

namespace Certus.Domain.Tests.RiskAndPortfolio;

public class AllocationTargetTests
{
    [Fact]
    public void Constructor_Should_Set_All_Properties()
    {
        var strategyId = Guid.NewGuid();

        var target = new AllocationTarget(strategyId, 0.4m, 0.1m, 0.7m);

        target.StrategyId.Should().Be(strategyId);
        target.TargetWeight.Should().Be(0.4m);
        target.MinWeight.Should().Be(0.1m);
        target.MaxWeight.Should().Be(0.7m);
    }

    [Fact]
    public void Record_Equality_SameValues_Should_Be_Equal()
    {
        var strategyId = Guid.NewGuid();

        var a = new AllocationTarget(strategyId, 0.4m, 0.1m, 0.7m);
        var b = new AllocationTarget(strategyId, 0.4m, 0.1m, 0.7m);

        a.Should().Be(b);
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void Record_Equality_DifferentValues_Should_Not_Be_Equal()
    {
        var strategyId = Guid.NewGuid();

        var a = new AllocationTarget(strategyId, 0.4m, 0.1m, 0.7m);
        var b = new AllocationTarget(strategyId, 0.5m, 0.1m, 0.7m);

        a.Should().NotBe(b);
        (a != b).Should().BeTrue();
    }
}
