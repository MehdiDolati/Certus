using Certus.Domain.Coordination.Enums;
using Certus.Domain.Coordination.Events;
using Certus.Domain.Coordination.ValueObjects;
using FluentAssertions;

namespace Certus.Domain.Tests.Coordination;

public class DomainEventTests
{
    [Fact]
    public void AgentStatusChanged_Should_Store_All_Properties()
    {
        var @event = new AgentStatusChanged
        {
            AgentType = AgentType.StrategyDesign,
            OldStatus = AgentStatus.Healthy,
            NewStatus = AgentStatus.Degraded
        };

        @event.EventId.Should().NotBe(Guid.Empty);
        @event.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        @event.AgentType.Should().Be(AgentType.StrategyDesign);
        @event.OldStatus.Should().Be(AgentStatus.Healthy);
        @event.NewStatus.Should().Be(AgentStatus.Degraded);
    }

    [Fact]
    public void AgentStatusChanged_Should_Allow_Custom_EventId()
    {
        var eventId = Guid.NewGuid();
        var @event = new AgentStatusChanged
        {
            EventId = eventId,
            AgentType = AgentType.Meta,
            OldStatus = AgentStatus.Offline,
            NewStatus = AgentStatus.Healthy
        };

        @event.EventId.Should().Be(eventId);
    }

    [Fact]
    public void CycleCompleted_Should_Store_All_Properties()
    {
        var cycleId = Guid.NewGuid();
        var metrics = new CycleMetrics(TimeSpan.FromSeconds(20), 2, 3, 1);
        var @event = new CycleCompleted
        {
            CycleId = cycleId,
            Metrics = metrics
        };

        @event.EventId.Should().NotBe(Guid.Empty);
        @event.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        @event.CycleId.Should().Be(cycleId);
        @event.Metrics.Should().Be(metrics);
    }

    [Fact]
    public void CycleStarted_Should_Store_All_Properties()
    {
        var cycleId = Guid.NewGuid();
        var @event = new CycleStarted
        {
            CycleId = cycleId,
            Phase = CyclePhase.DecisionMaking
        };

        @event.EventId.Should().NotBe(Guid.Empty);
        @event.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        @event.CycleId.Should().Be(cycleId);
        @event.Phase.Should().Be(CyclePhase.DecisionMaking);
    }

    [Fact]
    public void SystemAlert_Should_Store_All_Properties()
    {
        var @event = new SystemAlert
        {
            Severity = AlertSeverity.Critical,
            Message = "Agent unreachable",
            Source = "HealthMonitor"
        };

        @event.EventId.Should().NotBe(Guid.Empty);
        @event.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        @event.Severity.Should().Be(AlertSeverity.Critical);
        @event.Message.Should().Be("Agent unreachable");
        @event.Source.Should().Be("HealthMonitor");
    }

    [Fact]
    public void SystemAlert_Should_Allow_Null_Source()
    {
        var @event = new SystemAlert
        {
            Severity = AlertSeverity.Warning,
            Message = "High latency"
        };

        @event.Source.Should().BeNull();
    }

    [Fact]
    public void TaskAssigned_Should_Store_All_Properties()
    {
        var taskId = Guid.NewGuid();
        var @event = new TaskAssigned
        {
            TaskId = taskId,
            AgentType = AgentType.RiskPortfolio,
            Description = "Calculate risk metrics"
        };

        @event.EventId.Should().NotBe(Guid.Empty);
        @event.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        @event.TaskId.Should().Be(taskId);
        @event.AgentType.Should().Be(AgentType.RiskPortfolio);
        @event.Description.Should().Be("Calculate risk metrics");
    }

    [Fact]
    public void TaskAssigned_Should_Default_Description_To_Empty()
    {
        var @event = new TaskAssigned
        {
            TaskId = Guid.NewGuid(),
            AgentType = AgentType.Meta
        };

        @event.Description.Should().BeEmpty();
    }

    [Fact]
    public void TaskCompleted_Should_Store_All_Properties()
    {
        var taskId = Guid.NewGuid();
        var @event = new TaskCompleted
        {
            TaskId = taskId,
            Result = "Risk analysis complete"
        };

        @event.EventId.Should().NotBe(Guid.Empty);
        @event.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        @event.TaskId.Should().Be(taskId);
        @event.Result.Should().Be("Risk analysis complete");
    }

    [Fact]
    public void TaskCompleted_Should_Default_Result_To_Empty()
    {
        var @event = new TaskCompleted
        {
            TaskId = Guid.NewGuid()
        };

        @event.Result.Should().BeEmpty();
    }

    [Fact]
    public void AlertSeverity_Should_Have_Expected_Values()
    {
        AlertSeverity.Info.Should().Be((AlertSeverity)0);
        AlertSeverity.Warning.Should().Be((AlertSeverity)1);
        AlertSeverity.Critical.Should().Be((AlertSeverity)2);
    }
}
