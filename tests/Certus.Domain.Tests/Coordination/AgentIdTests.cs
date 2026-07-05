using Certus.Domain.Coordination.Enums;
using Certus.Domain.Coordination.ValueObjects;
using FluentAssertions;

namespace Certus.Domain.Tests.Coordination;

public class AgentIdTests
{
    [Fact]
    public void AgentId_Should_Store_Value_And_AgentType()
    {
        var agentId = new AgentId("agent-001", AgentType.MarketIntelligence);

        agentId.Value.Should().Be("agent-001");
        agentId.AgentType.Should().Be(AgentType.MarketIntelligence);
    }

    [Fact]
    public void AgentId_ToString_Should_Format_As_TypeColonValue()
    {
        var agentId = new AgentId("alpha", AgentType.StrategyDesign);

        agentId.ToString().Should().Be("StrategyDesign:alpha");
    }

    [Fact]
    public void AgentId_Equality_Should_Be_Based_On_Value_And_Type()
    {
        var id1 = new AgentId("agent-001", AgentType.TradeExecution);
        var id2 = new AgentId("agent-001", AgentType.TradeExecution);

        id1.Should().Be(id2);
    }

    [Fact]
    public void AgentId_Different_Values_Should_Not_Be_Equal()
    {
        var id1 = new AgentId("agent-001", AgentType.TradeExecution);
        var id2 = new AgentId("agent-002", AgentType.TradeExecution);

        id1.Should().NotBe(id2);
    }

    [Fact]
    public void AgentId_Different_Types_Should_Not_Be_Equal()
    {
        var id1 = new AgentId("agent-001", AgentType.TradeExecution);
        var id2 = new AgentId("agent-001", AgentType.RiskPortfolio);

        id1.Should().NotBe(id2);
    }
}
