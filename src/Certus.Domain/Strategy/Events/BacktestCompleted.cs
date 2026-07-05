using Certus.Domain.Strategy.ValueObjects;
using Certus.Domain.SharedKernel;

namespace Certus.Domain.Strategy.Events;

public record BacktestCompleted : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public required Guid StrategyId { get; init; }
    public required Guid RunId { get; init; }
    public required BacktestMetrics Metrics { get; init; }
}
