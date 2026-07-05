using Certus.Domain.RiskAndPortfolio.ValueObjects;
using FluentAssertions;

namespace Certus.Domain.Tests.RiskAndPortfolio;

public class PositionLimitTests
{
    [Fact]
    public void Constructor_Should_Set_All_Properties()
    {
        var limit = new PositionLimit(100_000m, 3m, 10);

        limit.MaxPositionSize.Should().Be(100_000m);
        limit.MaxLeverage.Should().Be(3m);
        limit.MaxConcurrentPositions.Should().Be(10);
    }

    [Fact]
    public void Record_Equality_SameValues_Should_Be_Equal()
    {
        var a = new PositionLimit(100_000m, 3m, 10);
        var b = new PositionLimit(100_000m, 3m, 10);

        a.Should().Be(b);
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void Record_Equality_DifferentValues_Should_Not_Be_Equal()
    {
        var a = new PositionLimit(100_000m, 3m, 10);
        var b = new PositionLimit(100_000m, 5m, 10);

        a.Should().NotBe(b);
        (a != b).Should().BeTrue();
    }
}
