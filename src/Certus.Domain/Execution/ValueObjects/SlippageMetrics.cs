using Certus.Domain.SharedKernel;

namespace Certus.Domain.Execution.ValueObjects;

public record SlippageMetrics 
{
    public decimal ExpectedPrice { get; }
    public decimal ActualPrice { get; }
    public decimal SlippagePercent { get; }
    public decimal SlippageAmount { get; }

    public SlippageMetrics(decimal expectedPrice, decimal actualPrice, decimal quantity)
    {
        ExpectedPrice = expectedPrice;
        ActualPrice = actualPrice;
        SlippageAmount = Math.Abs(actualPrice - expectedPrice) * quantity;
        SlippagePercent = expectedPrice > 0
            ? Math.Abs(actualPrice - expectedPrice) / expectedPrice * 100m
            : 0m;
    }

    public bool IsAcceptable => SlippagePercent < 0.5m;
    public bool IsSevere => SlippagePercent > 2m;
}
