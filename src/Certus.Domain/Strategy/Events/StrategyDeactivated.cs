using Certus.Domain.SharedKernel;

namespace Certus.Domain.Strategy.Events;

public record StrategyDeactivated : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public required Guid StrategyId { get; init; }
    public required string Reason { get; init; }
}
