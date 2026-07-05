using Certus.Domain.RiskAndPortfolio.Enums;
using Certus.Domain.SharedKernel;

namespace Certus.Domain.RiskAndPortfolio.Events;

public record RiskAssessmentCompleted : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public required Guid PortfolioId { get; init; }
    public required RiskLevel RiskLevel { get; init; }
    public decimal PortfolioVaR { get; init; }
    public decimal CurrentDrawdown { get; init; }
}
