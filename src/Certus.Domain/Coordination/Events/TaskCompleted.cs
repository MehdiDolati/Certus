using Certus.Domain.SharedKernel;

namespace Certus.Domain.Coordination.Events;

public record TaskCompleted : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public required Guid TaskId { get; init; }
    public string Result { get; init; } = string.Empty;
}
