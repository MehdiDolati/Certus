using Certus.Domain.RiskAndPortfolio.Enums;
using Certus.Domain.SharedKernel;

namespace Certus.Domain.RiskAndPortfolio.Events;

public record RiskLimitBreached : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public required Guid PortfolioId { get; init; }
    public required RiskLimitType LimitType { get; init; }
    public required RiskLevel Severity { get; init; }
    public decimal Threshold { get; init; }
    public decimal CurrentValue { get; init; }
}
