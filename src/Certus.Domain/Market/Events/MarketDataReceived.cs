using Certus.Domain.Market.ValueObjects;
using Certus.Domain.SharedKernel;

namespace Certus.Domain.Market.Events;

public record MarketDataReceived : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public required Symbol Symbol { get; init; }
    public required TimeFrame TimeFrame { get; init; }
    public int BarCount { get; init; }
}
