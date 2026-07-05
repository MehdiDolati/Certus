using Certus.Domain.Coordination.Enums;
using Certus.Domain.Coordination.ValueObjects;
using Certus.Domain.SharedKernel;

namespace Certus.Domain.Coordination.Entities;

public class CoordinationCycle : Entity
{
    public Guid SystemHealthId { get; private set; }
    public CyclePhase Phase { get; private set; }
    public CycleMetrics Metrics { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private CoordinationCycle() { }

    public CoordinationCycle(
        Guid id,
        Guid systemHealthId,
        CyclePhase phase) : base(id)
    {
        SystemHealthId = systemHealthId;
        Phase = phase;
        Metrics = new CycleMetrics(TimeSpan.Zero, 0, 0, 0);
        StartedAt = DateTime.UtcNow;
    }

    public void Complete(CycleMetrics metrics)
    {
        CompletedAt = DateTime.UtcNow;
        Metrics = metrics;
    }
}
