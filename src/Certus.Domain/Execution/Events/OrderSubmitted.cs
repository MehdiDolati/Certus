using Certus.Domain.Execution.Enums;
using Certus.Domain.SharedKernel;

namespace Certus.Domain.Execution.Events;

public record OrderSubmitted : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public required Guid TradeId { get; init; }
    public required Guid OrderId { get; init; }
    public required Symbol Symbol { get; init; }
    public required TradeSide Side { get; init; }
}
