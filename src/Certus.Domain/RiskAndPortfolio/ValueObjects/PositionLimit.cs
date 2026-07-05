using Certus.Domain.SharedKernel;

namespace Certus.Domain.RiskAndPortfolio.ValueObjects;

public record PositionLimit 
{
    public decimal MaxPositionSize { get; }
    public decimal MaxLeverage { get; }
    public int MaxConcurrentPositions { get; }

    public PositionLimit(
        decimal maxPositionSize,
        decimal maxLeverage,
        int maxConcurrentPositions)
    {
        MaxPositionSize = maxPositionSize;
        MaxLeverage = maxLeverage;
        MaxConcurrentPositions = maxConcurrentPositions;
    }
}
