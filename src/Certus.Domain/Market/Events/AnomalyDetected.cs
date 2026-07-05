using Certus.Domain.SharedKernel;

namespace Certus.Domain.Market.Events;

public enum AnomalyType
{
    PriceSpike = 0,
    VolumeSpike = 1,
    SpreadWidening = 2,
    LiquidityDrop = 3,
    CorrelationBreak = 4
}

public record AnomalyDetected : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public required Symbol Symbol { get; init; }
    public required AnomalyType AnomalyType { get; init; }
    public decimal Severity { get; init; }
    public string Description { get; init; } = string.Empty;
}
