using Certus.Domain.SharedKernel;

namespace Certus.Domain.RiskAndPortfolio.Events;

public record PortfolioCreated : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public required Guid PortfolioId { get; init; }
    public required string Name { get; init; }
    public required Money AllocatedCapital { get; init; }
}
