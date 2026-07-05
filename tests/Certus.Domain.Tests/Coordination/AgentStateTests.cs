using Certus.Domain.Coordination.Entities;
using Certus.Domain.Coordination.Enums;
using FluentAssertions;

namespace Certus.Domain.Tests.Coordination;

public class AgentStateTests
{
    [Fact]
    public void AgentState_Should_Be_Created_With_Correct_Defaults()
    {
        var id = Guid.NewGuid();
        var systemHealthId = Guid.NewGuid();
        var agentType = AgentType.TradeExecution;
        var status = AgentStatus.Healthy;

        var agentState = new AgentState(id, systemHealthId, agentType, status);

        agentState.Id.Should().Be(id);
        agentState.SystemHealthId.Should().Be(systemHealthId);
        agentState.AgentType.Should().Be(agentType);
        agentState.Status.Should().Be(status);
        agentState.LastHeartbeat.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        agentState.Message.Should().BeNull();
    }

    [Fact]
    public void AgentState_Should_Be_Created_With_Message()
    {
        var agentState = new AgentState(
            Guid.NewGuid(), Guid.NewGuid(),
            AgentType.EvaluationFeedback, AgentStatus.Healthy, "All systems operational");

        agentState.Message.Should().Be("All systems operational");
    }

    [Fact]
    public void AgentState_UpdateStatus_Should_Change_Status()
    {
        var agentState = CreateDefaultAgentState();

        agentState.UpdateStatus(AgentStatus.Degraded);

        agentState.Status.Should().Be(AgentStatus.Degraded);
    }

    [Fact]
    public void AgentState_UpdateStatus_Should_Update_Heartbeat()
    {
        var agentState = CreateDefaultAgentState();
        var beforeUpdate = DateTime.UtcNow;

        agentState.UpdateStatus(AgentStatus.Offline);

        agentState.LastHeartbeat.Should().BeOnOrAfter(beforeUpdate);
    }

    [Fact]
    public void AgentState_UpdateStatus_Should_Set_Message()
    {
        var agentState = CreateDefaultAgentState();

        agentState.UpdateStatus(AgentStatus.Degraded, "High latency detected");

        agentState.Message.Should().Be("High latency detected");
    }

    [Fact]
    public void AgentState_UpdateStatus_Should_Clear_Message_When_Null()
    {
        var agentState = new AgentState(
            Guid.NewGuid(), Guid.NewGuid(),
            AgentType.MarketIntelligence, AgentStatus.Healthy, "Initial message");

        agentState.UpdateStatus(AgentStatus.Degraded, null);

        agentState.Message.Should().BeNull();
    }

    [Fact]
    public void AgentState_Equality_Should_Be_Based_On_Id()
    {
        var id = Guid.NewGuid();
        var state1 = new AgentState(id, Guid.NewGuid(), AgentType.Meta, AgentStatus.Healthy);
        var state2 = new AgentState(id, Guid.NewGuid(), AgentType.Meta, AgentStatus.Offline);

        state1.Should().Be(state2);
    }

    private static AgentState CreateDefaultAgentState() =>
        new(Guid.NewGuid(), Guid.NewGuid(), AgentType.StrategyDesign, AgentStatus.Healthy);
}
