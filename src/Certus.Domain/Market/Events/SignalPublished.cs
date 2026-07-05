using Certus.Domain.Market.Enums;
using Certus.Domain.Market.ValueObjects;
using Certus.Domain.SharedKernel;

namespace Certus.Domain.Market.Events;

public record SignalPublished : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public required Guid SignalId { get; init; }
    public required Symbol Symbol { get; init; }
    public required SignalDirection Direction { get; init; }
    public required SignalStrength Strength { get; init; }
    public required IndicatorType IndicatorType { get; init; }
}
