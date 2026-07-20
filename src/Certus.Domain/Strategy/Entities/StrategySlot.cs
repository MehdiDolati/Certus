using Certus.Domain.Strategy.ValueObjects;
using Certus.Domain.SharedKernel;

namespace Certus.Domain.Strategy.Entities;

public class StrategySlot : Entity
{
    public Guid StrategyId { get; private set; }
    public Guid PortfolioId { get; private set; }
    public Weight Weight { get; private set; }
    public DateTime AssignedAt { get; private set; }

    private StrategySlot() { }

    public StrategySlot(Guid id, Guid strategyId, Guid portfolioId, Weight weight) : base(id)
    {
        StrategyId = strategyId;
        PortfolioId = portfolioId;
        Weight = weight;
        AssignedAt = DateTime.UtcNow;
    }

    public void UpdateWeight(Weight newWeight)
    {
        Weight = newWeight;
    }
}
