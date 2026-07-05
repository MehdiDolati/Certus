using Certus.Domain.Strategy.ValueObjects;
using Certus.Domain.SharedKernel;

namespace Certus.Domain.Strategy.Events;

public record StrategyActivated : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public required Guid StrategyId { get; init; }
    public required string Name { get; init; }
    public required StrategyType Type { get; init; }
}
