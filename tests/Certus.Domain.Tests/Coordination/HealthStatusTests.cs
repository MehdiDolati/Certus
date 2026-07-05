using Certus.Domain.Coordination.Enums;
using Certus.Domain.Coordination.ValueObjects;
using FluentAssertions;

namespace Certus.Domain.Tests.Coordination;

public class HealthStatusTests
{
    [Fact]
    public void HealthStatus_Should_Store_All_Properties()
    {
        var statuses = new List<AgentStatusInfo>
        {
            new(AgentType.MarketIntelligence, AgentStatus.Healthy, DateTime.UtcNow),
            new(AgentType.TradeExecution, AgentStatus.Degraded, DateTime.UtcNow)
        };
        var lastCheck = DateTime.UtcNow;

        var healthStatus = new HealthStatus(AgentStatus.Degraded, statuses, lastCheck);

        healthStatus.Overall.Should().Be(AgentStatus.Degraded);
        healthStatus.AgentStatuses.Should().HaveCount(2);
        healthStatus.LastCheck.Should().Be(lastCheck);
    }

    [Fact]
    public void HealthStatus_Should_Accept_Empty_AgentStatuses()
    {
        var healthStatus = new HealthStatus(AgentStatus.Healthy, [], DateTime.UtcNow);

        healthStatus.AgentStatuses.Should().BeEmpty();
    }

    [Fact]
    public void HealthStatus_Same_List_Reference_Should_Be_Equal()
    {
        var lastCheck = DateTime.UtcNow;
        var statuses = new List<AgentStatusInfo>
        {
            new(AgentType.Meta, AgentStatus.Healthy, DateTime.UtcNow)
        };

        var h1 = new HealthStatus(AgentStatus.Healthy, statuses, lastCheck);
        var h2 = new HealthStatus(AgentStatus.Healthy, statuses, lastCheck);

        h1.Should().Be(h2);
    }

    [Fact]
    public void HealthStatus_Different_List_References_Should_Not_Be_Equal()
    {
        var lastCheck = DateTime.UtcNow;
        var statuses1 = new List<AgentStatusInfo>
        {
            new(AgentType.Meta, AgentStatus.Healthy, DateTime.UtcNow)
        };
        var statuses2 = new List<AgentStatusInfo>
        {
            new(AgentType.Meta, AgentStatus.Healthy, statuses1[0].LastHeartbeat)
        };

        var h1 = new HealthStatus(AgentStatus.Healthy, statuses1, lastCheck);
        var h2 = new HealthStatus(AgentStatus.Healthy, statuses2, lastCheck);

        h1.Should().NotBe(h2);
    }

    [Fact]
    public void HealthStatus_Different_Overall_Should_Not_Be_Equal()
    {
        var h1 = new HealthStatus(AgentStatus.Healthy, [], DateTime.UtcNow);
        var h2 = new HealthStatus(AgentStatus.Degraded, [], DateTime.UtcNow);

        h1.Should().NotBe(h2);
    }

    [Fact]
    public void AgentStatusInfo_Should_Store_All_Properties()
    {
        var lastHeartbeat = DateTime.UtcNow;
        var info = new AgentStatusInfo(AgentType.RiskPortfolio, AgentStatus.Offline, lastHeartbeat);

        info.AgentType.Should().Be(AgentType.RiskPortfolio);
        info.Status.Should().Be(AgentStatus.Offline);
        info.LastHeartbeat.Should().Be(lastHeartbeat);
    }

    [Fact]
    public void AgentStatusInfo_Equality_Should_Be_Based_On_All_Properties()
    {
        var hb = DateTime.UtcNow;
        var i1 = new AgentStatusInfo(AgentType.Meta, AgentStatus.Healthy, hb);
        var i2 = new AgentStatusInfo(AgentType.Meta, AgentStatus.Healthy, hb);

        i1.Should().Be(i2);
    }

    [Fact]
    public void AgentStatusInfo_Different_AgentType_Should_Not_Be_Equal()
    {
        var hb = DateTime.UtcNow;
        var i1 = new AgentStatusInfo(AgentType.Meta, AgentStatus.Healthy, hb);
        var i2 = new AgentStatusInfo(AgentType.TradeExecution, AgentStatus.Healthy, hb);

        i1.Should().NotBe(i2);
    }
}
