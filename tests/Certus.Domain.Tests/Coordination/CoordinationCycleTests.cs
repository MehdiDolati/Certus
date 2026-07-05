using Certus.Domain.Coordination.Entities;
using Certus.Domain.Coordination.Enums;
using Certus.Domain.Coordination.ValueObjects;
using FluentAssertions;

namespace Certus.Domain.Tests.Coordination;

public class CoordinationCycleTests
{
    [Fact]
    public void CoordinationCycle_Should_Be_Created_With_Correct_Defaults()
    {
        var id = Guid.NewGuid();
        var systemHealthId = Guid.NewGuid();
        var phase = CyclePhase.DataGathering;

        var cycle = new CoordinationCycle(id, systemHealthId, phase);

        cycle.Id.Should().Be(id);
        cycle.SystemHealthId.Should().Be(systemHealthId);
        cycle.Phase.Should().Be(phase);
        cycle.Metrics.Duration.Should().Be(TimeSpan.Zero);
        cycle.Metrics.AgentsParticipated.Should().Be(0);
        cycle.Metrics.DecisionsMade.Should().Be(0);
        cycle.Metrics.TradesExecuted.Should().Be(0);
        cycle.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        cycle.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void CoordinationCycle_Complete_Should_Set_CompletedAt_And_Metrics()
    {
        var cycle = CreateDefaultCycle();
        var metrics = new CycleMetrics(TimeSpan.FromSeconds(30), 3, 5, 2);

        cycle.Complete(metrics);

        cycle.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        cycle.Metrics.Duration.Should().Be(TimeSpan.FromSeconds(30));
        cycle.Metrics.AgentsParticipated.Should().Be(3);
        cycle.Metrics.DecisionsMade.Should().Be(5);
        cycle.Metrics.TradesExecuted.Should().Be(2);
    }

    [Fact]
    public void CoordinationCycle_Equality_Should_Be_Based_On_Id()
    {
        var id = Guid.NewGuid();
        var cycle1 = new CoordinationCycle(id, Guid.NewGuid(), CyclePhase.Analysis);
        var cycle2 = new CoordinationCycle(id, Guid.NewGuid(), CyclePhase.Execution);

        cycle1.Should().Be(cycle2);
    }

    private static CoordinationCycle CreateDefaultCycle() =>
        new(Guid.NewGuid(), Guid.NewGuid(), CyclePhase.DataGathering);
}
