using Certus.Domain.SharedKernel;

namespace Certus.Domain.RiskAndPortfolio.Entities;

public class AllocationRule : Entity
{
    public Guid PortfolioId { get; private set; }
    public Guid StrategyId { get; private set; }
    public decimal TargetWeight { get; private set; }
    public decimal MinWeight { get; private set; }
    public decimal MaxWeight { get; private set; }
    public bool IsActive { get; private set; }

    private AllocationRule() { }

    public AllocationRule(
        Guid id,
        Guid portfolioId,
        Guid strategyId,
        decimal targetWeight,
        decimal minWeight = 0m,
        decimal maxWeight = 1m) : base(id)
    {
        PortfolioId = portfolioId;
        StrategyId = strategyId;
        TargetWeight = targetWeight;
        MinWeight = minWeight;
        MaxWeight = maxWeight;
        IsActive = true;
    }

    public void UpdateWeight(decimal targetWeight, decimal minWeight, decimal maxWeight)
    {
        if (targetWeight < minWeight || targetWeight > maxWeight)
            throw new ArgumentException("Target weight must be within min/max range");

        TargetWeight = targetWeight;
        MinWeight = minWeight;
        MaxWeight = maxWeight;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
