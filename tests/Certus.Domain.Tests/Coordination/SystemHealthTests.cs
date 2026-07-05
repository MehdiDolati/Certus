using Certus.Domain.Coordination.Aggregates;
using Certus.Domain.Coordination.Entities;
using Certus.Domain.Coordination.Enums;
using FluentAssertions;

namespace Certus.Domain.Tests.Coordination;

public class SystemHealthTests
{
    [Fact]
    public void SystemHealth_Should_Be_Created_With_Correct_Defaults()
    {
        var id = Guid.NewGuid();

        var health = new SystemHealth(id);

        health.Id.Should().Be(id);
        health.OverallStatus.Should().Be(AgentStatus.Healthy);
        health.LastChecked.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        health.AgentStates.Should().BeEmpty();
        health.Cycles.Should().BeEmpty();
    }

    [Fact]
    public void SystemHealth_UpdateAgentState_Should_Add_New_AgentState()
    {
        var health = new SystemHealth(Guid.NewGuid());

        health.UpdateAgentState(AgentType.MarketIntelligence, AgentStatus.Healthy);

        health.AgentStates.Should().HaveCount(1);
        health.AgentStates[0].AgentType.Should().Be(AgentType.MarketIntelligence);
        health.AgentStates[0].Status.Should().Be(AgentStatus.Healthy);
    }

    [Fact]
    public void SystemHealth_UpdateAgentState_Should_Replace_Existing_AgentState_For_Same_Type()
    {
        var health = new SystemHealth(Guid.NewGuid());

        health.UpdateAgentState(AgentType.MarketIntelligence, AgentStatus.Healthy);
        health.UpdateAgentState(AgentType.MarketIntelligence, AgentStatus.Degraded);

        health.AgentStates.Should().HaveCount(1);
        health.AgentStates[0].Status.Should().Be(AgentStatus.Degraded);
    }

    [Fact]
    public void SystemHealth_UpdateAgentState_Should_Update_LastChecked()
    {
        var health = new SystemHealth(Guid.NewGuid());
        var beforeUpdate = DateTime.UtcNow;

        health.UpdateAgentState(AgentType.MarketIntelligence, AgentStatus.Healthy);

        health.LastChecked.Should().BeOnOrAfter(beforeUpdate);
    }

    [Fact]
    public void SystemHealth_OverallStatus_Should_Be_Healthy_When_All_Agents_Healthy()
    {
        var health = new SystemHealth(Guid.NewGuid());

        health.UpdateAgentState(AgentType.MarketIntelligence, AgentStatus.Healthy);
        health.UpdateAgentState(AgentType.StrategyDesign, AgentStatus.Healthy);
        health.UpdateAgentState(AgentType.RiskPortfolio, AgentStatus.Healthy);

        health.OverallStatus.Should().Be(AgentStatus.Healthy);
    }

    [Fact]
    public void SystemHealth_OverallStatus_Should_Be_Degraded_When_Any_Agent_Offline()
    {
        var health = new SystemHealth(Guid.NewGuid());

        health.UpdateAgentState(AgentType.MarketIntelligence, AgentStatus.Healthy);
        health.UpdateAgentState(AgentType.TradeExecution, AgentStatus.Offline);

        health.OverallStatus.Should().Be(AgentStatus.Degraded);
    }

    [Fact]
    public void SystemHealth_OverallStatus_Should_Be_Degraded_When_Mixed_Statuses()
    {
        var health = new SystemHealth(Guid.NewGuid());

        health.UpdateAgentState(AgentType.MarketIntelligence, AgentStatus.Healthy);
        health.UpdateAgentState(AgentType.StrategyDesign, AgentStatus.Degraded);

        health.OverallStatus.Should().Be(AgentStatus.Degraded);
    }

    [Fact]
    public void SystemHealth_AddCycle_Should_Add_Cycle_To_Collection()
    {
        var health = new SystemHealth(Guid.NewGuid());
        var cycle = new CoordinationCycle(Guid.NewGuid(), health.Id, CyclePhase.DataGathering);

        health.AddCycle(cycle);

        health.Cycles.Should().HaveCount(1);
        health.Cycles[0].Should().Be(cycle);
    }

    [Fact]
    public void SystemHealth_AddMultipleCycles_Should_All_Be_Added()
    {
        var health = new SystemHealth(Guid.NewGuid());

        health.AddCycle(new CoordinationCycle(Guid.NewGuid(), health.Id, CyclePhase.DataGathering));
        health.AddCycle(new CoordinationCycle(Guid.NewGuid(), health.Id, CyclePhase.Analysis));

        health.Cycles.Should().HaveCount(2);
    }

    [Fact]
    public void SystemHealth_AgentStates_Should_Be_ReadOnly()
    {
        var health = new SystemHealth(Guid.NewGuid());

        health.AgentStates.Should().BeAssignableTo<IReadOnlyList<AgentState>>();
    }

    [Fact]
    public void SystemHealth_Cycles_Should_Be_ReadOnly()
    {
        var health = new SystemHealth(Guid.NewGuid());

        health.Cycles.Should().BeAssignableTo<IReadOnlyList<CoordinationCycle>>();
    }

    [Fact]
    public void SystemHealth_Equality_Should_Be_Based_On_Id()
    {
        var id = Guid.NewGuid();
        var health1 = new SystemHealth(id);
        var health2 = new SystemHealth(id);

        health1.Should().Be(health2);
    }
}
