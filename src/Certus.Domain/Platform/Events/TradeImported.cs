using Certus.Domain.SharedKernel;

namespace Certus.Domain.Platform.Events;

public record TradeImported : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid TradeId { get; init; }
    public Guid StrategyId { get; init; }
    public string ExternalId { get; init; } = string.Empty;
    public decimal PnL { get; init; }
}
