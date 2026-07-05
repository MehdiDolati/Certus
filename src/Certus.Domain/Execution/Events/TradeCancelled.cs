using Certus.Domain.SharedKernel;

namespace Certus.Domain.Execution.Events;

public record TradeCancelled : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public required Guid TradeId { get; init; }
    public required string Reason { get; init; }
}
