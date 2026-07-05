using Certus.Domain.SharedKernel;

namespace Certus.Domain.RiskAndPortfolio.Entities;

public class CapitalAllocation : Entity
{
    public Guid PortfolioId { get; private set; }
    public Guid StrategyId { get; private set; }
    public Money AllocatedAmount { get; private set; }
    public decimal Weight { get; private set; }
    public DateTime AllocatedAt { get; private set; }

    private CapitalAllocation() { }

    public CapitalAllocation(
        Guid id,
        Guid portfolioId,
        Guid strategyId,
        Money allocatedAmount,
        decimal weight) : base(id)
    {
        PortfolioId = portfolioId;
        StrategyId = strategyId;
        AllocatedAmount = allocatedAmount;
        Weight = weight;
        AllocatedAt = DateTime.UtcNow;
    }

    public void UpdateAllocation(Money newAmount, decimal newWeight)
    {
        AllocatedAmount = newAmount;
        Weight = newWeight;
    }
}
