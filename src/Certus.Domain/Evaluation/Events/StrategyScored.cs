using Certus.Domain.Evaluation.ValueObjects;
using Certus.Domain.SharedKernel;

namespace Certus.Domain.Evaluation.Events;

public record StrategyScored : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public required Guid StrategyId { get; init; }
    public required EvaluationScore Score { get; init; }
    public string Recommendation { get; init; } = string.Empty;
}
