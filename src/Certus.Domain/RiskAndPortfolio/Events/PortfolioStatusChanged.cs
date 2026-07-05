using Certus.Domain.RiskAndPortfolio.Enums;
using Certus.Domain.SharedKernel;

namespace Certus.Domain.RiskAndPortfolio.Events;

public record PortfolioStatusChanged : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public required Guid PortfolioId { get; init; }
    public required PortfolioStatus OldStatus { get; init; }
    public required PortfolioStatus NewStatus { get; init; }
}
