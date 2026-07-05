using Certus.Domain.SharedKernel;

namespace Certus.Domain.Coordination.Events;

public enum AlertSeverity
{
    Info = 0,
    Warning = 1,
    Critical = 2
}

public record SystemAlert : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public required AlertSeverity Severity { get; init; }
    public required string Message { get; init; }
    public string? Source { get; init; }
}
