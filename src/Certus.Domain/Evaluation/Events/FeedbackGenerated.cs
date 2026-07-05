using Certus.Domain.Evaluation.Enums;
using Certus.Domain.SharedKernel;

namespace Certus.Domain.Evaluation.Events;

public record FeedbackGenerated : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public required Guid StrategyId { get; init; }
    public required FeedbackType FeedbackType { get; init; }
    public required string Message { get; init; }
}
