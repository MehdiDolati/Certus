using Certus.Domain.SharedKernel;

namespace Certus.Domain.Execution.Events;

public record SlippageReported : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public required Guid TradeId { get; init; }
    public decimal ExpectedPrice { get; init; }
    public decimal ActualPrice { get; init; }
    public decimal SlippagePercent { get; init; }
}
