using Certus.Domain.SharedKernel;

namespace Certus.Domain.Coordination.ValueObjects;

public record CycleMetrics 
{
    public TimeSpan Duration { get; }
    public int AgentsParticipated { get; }
    public int DecisionsMade { get; }
    public int TradesExecuted { get; }

    public CycleMetrics(
        TimeSpan duration,
        int agentsParticipated,
        int decisionsMade,
        int tradesExecuted)
    {
        Duration = duration;
        AgentsParticipated = agentsParticipated;
        DecisionsMade = decisionsMade;
        TradesExecuted = tradesExecuted;
    }
}
