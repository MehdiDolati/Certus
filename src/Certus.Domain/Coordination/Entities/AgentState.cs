using Certus.Domain.Coordination.Enums;
using Certus.Domain.SharedKernel;

namespace Certus.Domain.Coordination.Entities;

public class AgentState : Entity
{
    public Guid SystemHealthId { get; private set; }
    public AgentType AgentType { get; private set; }
    public AgentStatus Status { get; private set; }
    public DateTime LastHeartbeat { get; private set; }
    public string? Message { get; private set; }

    private AgentState() { }

    public AgentState(
        Guid id,
        Guid systemHealthId,
        AgentType agentType,
        AgentStatus status,
        string? message = null) : base(id)
    {
        SystemHealthId = systemHealthId;
        AgentType = agentType;
        Status = status;
        Message = message;
        LastHeartbeat = DateTime.UtcNow;
    }

    public void UpdateStatus(AgentStatus status, string? message = null)
    {
        Status = status;
        Message = message;
        LastHeartbeat = DateTime.UtcNow;
    }
}
