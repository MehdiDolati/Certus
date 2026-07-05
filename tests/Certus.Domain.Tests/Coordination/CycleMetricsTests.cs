using Certus.Domain.Coordination.ValueObjects;
using FluentAssertions;

namespace Certus.Domain.Tests.Coordination;

public class CycleMetricsTests
{
    [Fact]
    public void CycleMetrics_Should_Store_All_Properties()
    {
        var metrics = new CycleMetrics(TimeSpan.FromSeconds(45), 4, 8, 3);

        metrics.Duration.Should().Be(TimeSpan.FromSeconds(45));
        metrics.AgentsParticipated.Should().Be(4);
        metrics.DecisionsMade.Should().Be(8);
        metrics.TradesExecuted.Should().Be(3);
    }

    [Fact]
    public void CycleMetrics_Should_Accept_Zero_Values()
    {
        var metrics = new CycleMetrics(TimeSpan.Zero, 0, 0, 0);

        metrics.Duration.Should().Be(TimeSpan.Zero);
        metrics.AgentsParticipated.Should().Be(0);
        metrics.DecisionsMade.Should().Be(0);
        metrics.TradesExecuted.Should().Be(0);
    }

    [Fact]
    public void CycleMetrics_Equality_Should_Be_Based_On_All_Properties()
    {
        var m1 = new CycleMetrics(TimeSpan.FromSeconds(30), 3, 5, 2);
        var m2 = new CycleMetrics(TimeSpan.FromSeconds(30), 3, 5, 2);

        m1.Should().Be(m2);
    }

    [Fact]
    public void CycleMetrics_Different_Duration_Should_Not_Be_Equal()
    {
        var m1 = new CycleMetrics(TimeSpan.FromSeconds(30), 3, 5, 2);
        var m2 = new CycleMetrics(TimeSpan.FromSeconds(60), 3, 5, 2);

        m1.Should().NotBe(m2);
    }

    [Fact]
    public void CycleMetrics_Different_Participants_Should_Not_Be_Equal()
    {
        var m1 = new CycleMetrics(TimeSpan.FromSeconds(30), 3, 5, 2);
        var m2 = new CycleMetrics(TimeSpan.FromSeconds(30), 5, 5, 2);

        m1.Should().NotBe(m2);
    }

    [Fact]
    public void CycleMetrics_Different_Decisions_Should_Not_Be_Equal()
    {
        var m1 = new CycleMetrics(TimeSpan.FromSeconds(30), 3, 5, 2);
        var m2 = new CycleMetrics(TimeSpan.FromSeconds(30), 3, 8, 2);

        m1.Should().NotBe(m2);
    }

    [Fact]
    public void CycleMetrics_Different_Trades_Should_Not_Be_Equal()
    {
        var m1 = new CycleMetrics(TimeSpan.FromSeconds(30), 3, 5, 2);
        var m2 = new CycleMetrics(TimeSpan.FromSeconds(30), 3, 5, 7);

        m1.Should().NotBe(m2);
    }
}
