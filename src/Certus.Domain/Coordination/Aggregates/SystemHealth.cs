using Certus.Domain.Coordination.Entities;
using Certus.Domain.Coordination.Enums;
using Certus.Domain.SharedKernel;

namespace Certus.Domain.Coordination.Aggregates;

public class SystemHealth : AggregateRoot
{
    private readonly List<AgentState> _agentStates = [];
    private readonly List<CoordinationCycle> _cycles = [];

    public IReadOnlyList<AgentState> AgentStates => _agentStates.AsReadOnly();
    public IReadOnlyList<CoordinationCycle> Cycles => _cycles.AsReadOnly();

    public AgentStatus OverallStatus { get; private set; }
    public DateTime LastChecked { get; private set; }

    private SystemHealth() { }

    public SystemHealth(Guid id) : base(id)
    {
        OverallStatus = AgentStatus.Healthy;
        LastChecked = DateTime.UtcNow;
    }

    public void UpdateAgentState(AgentType agentType, AgentStatus status)
    {
        var existing = _agentStates.FirstOrDefault(a => a.AgentType == agentType);
        if (existing is not null)
        {
            _agentStates.Remove(existing);
        }

        var newState = new AgentState(Guid.NewGuid(), Id, agentType, status);
        _agentStates.Add(newState);
        RecalculateOverallStatus();
        LastChecked = DateTime.UtcNow;
    }

    public void AddCycle(CoordinationCycle cycle)
    {
        _cycles.Add(cycle);
    }

    private void RecalculateOverallStatus()
    {
        if (_agentStates.Any(a => a.Status == AgentStatus.Offline))
            OverallStatus = AgentStatus.Degraded;
        else if (_agentStates.All(a => a.Status == AgentStatus.Healthy))
            OverallStatus = AgentStatus.Healthy;
        else
            OverallStatus = AgentStatus.Degraded;
    }
}
