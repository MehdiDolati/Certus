using Certus.Domain.SharedKernel;

namespace Certus.Domain.Strategy.ValueObjects;

public record RiskProfile 
{
    public decimal MaxDrawdownLimit { get; }
    public decimal MaxPositionSize { get; }
    public decimal VolatilityTarget { get; }

    public RiskProfile(
        decimal maxDrawdownLimit,
        decimal maxPositionSize,
        decimal volatilityTarget)
    {
        if (maxDrawdownLimit < 0 || maxDrawdownLimit > 1)
            throw new ArgumentOutOfRangeException(nameof(maxDrawdownLimit));
        if (maxPositionSize < 0 || maxPositionSize > 1)
            throw new ArgumentOutOfRangeException(nameof(maxPositionSize));

        MaxDrawdownLimit = maxDrawdownLimit;
        MaxPositionSize = maxPositionSize;
        VolatilityTarget = volatilityTarget;
    }
}
