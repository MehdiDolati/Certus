using Certus.Domain.Evaluation.ValueObjects;
using Certus.Domain.SharedKernel;

namespace Certus.Domain.Evaluation.Events;

public record PerformanceEvaluated : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public required Guid ReportId { get; init; }
    public required Guid PortfolioId { get; init; }
    public required ReturnMetrics ReturnMetrics { get; init; }
    public required RiskMetrics RiskMetrics { get; init; }
}
