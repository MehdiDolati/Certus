using Certus.Domain.Market.Enums;
using Certus.Domain.Market.ValueObjects;
using Certus.Domain.SharedKernel;

namespace Certus.Domain.Market.Aggregates;

public class Signal : Entity
{
    public Symbol Symbol { get; private set; }
    public SignalDirection Direction { get; private set; }
    public SignalStrength Strength { get; private set; }
    public IndicatorType IndicatorType { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public DateTime GeneratedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;

    private Signal() { }

    public Signal(
        Guid id,
        Symbol symbol,
        SignalDirection direction,
        SignalStrength strength,
        IndicatorType indicatorType,
        string reason,
        DateTime? expiresAt = null) : base(id)
    {
        Symbol = symbol;
        Direction = direction;
        Strength = strength;
        IndicatorType = indicatorType;
        Reason = reason;
        GeneratedAt = DateTime.UtcNow;
        ExpiresAt = expiresAt;
    }
}
