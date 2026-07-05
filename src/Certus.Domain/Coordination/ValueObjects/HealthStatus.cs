using Certus.Domain.Coordination.Enums;
using Certus.Domain.SharedKernel;

namespace Certus.Domain.Coordination.ValueObjects;

public record HealthStatus 
{
    public Coordination.Enums.AgentStatus Overall { get; }
    public IReadOnlyList<AgentStatusInfo> AgentStatuses { get; }
    public DateTime LastCheck { get; }

    public HealthStatus(
        Coordination.Enums.AgentStatus overall,
        IReadOnlyList<AgentStatusInfo> agentStatuses,
        DateTime lastCheck)
    {
        Overall = overall;
        AgentStatuses = agentStatuses;
        LastCheck = lastCheck;
    }
}

public record AgentStatusInfo(AgentType AgentType, AgentStatus Status, DateTime LastHeartbeat);
