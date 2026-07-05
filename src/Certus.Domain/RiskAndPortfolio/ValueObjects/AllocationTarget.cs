using Certus.Domain.SharedKernel;

namespace Certus.Domain.RiskAndPortfolio.ValueObjects;

public record AllocationTarget 
{
    public Guid StrategyId { get; }
    public decimal TargetWeight { get; }
    public decimal MinWeight { get; }
    public decimal MaxWeight { get; }

    public AllocationTarget(
        Guid strategyId,
        decimal targetWeight,
        decimal minWeight,
        decimal maxWeight)
    {
        StrategyId = strategyId;
        TargetWeight = targetWeight;
        MinWeight = minWeight;
        MaxWeight = maxWeight;
    }
}
