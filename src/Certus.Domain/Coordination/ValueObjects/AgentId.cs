using Certus.Domain.Coordination.Enums;
using Certus.Domain.SharedKernel;

namespace Certus.Domain.Coordination.ValueObjects;

public record AgentId 
{
    public string Value { get; }
    public AgentType AgentType { get; }

    public AgentId(string value, AgentType agentType)
    {
        Value = value;
        AgentType = agentType;
    }

    public override string ToString() => $"{AgentType}:{Value}";
}
