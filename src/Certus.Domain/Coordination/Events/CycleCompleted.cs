using Certus.Domain.Coordination.ValueObjects;
using Certus.Domain.SharedKernel;

namespace Certus.Domain.Coordination.Events;

public record CycleCompleted : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public required Guid CycleId { get; init; }
    public required CycleMetrics Metrics { get; init; }
}
