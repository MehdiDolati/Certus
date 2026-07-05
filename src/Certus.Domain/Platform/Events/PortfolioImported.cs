using Certus.Domain.SharedKernel;

namespace Certus.Domain.Platform.Events;

public record PortfolioImported : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid PortfolioId { get; init; }
    public Guid ConnectionId { get; init; }
    public string ExternalId { get; init; } = string.Empty;
}
