using Certus.Domain.RiskAndPortfolio.Enums;
using Certus.Domain.SharedKernel;

namespace Certus.Domain.RiskAndPortfolio.Entities;

public class RiskLimit : Entity
{
    public Guid PortfolioId { get; private set; }
    public RiskLimitType Type { get; private set; }
    public decimal Threshold { get; private set; }
    public decimal CurrentValue { get; private set; }
    public bool IsBreached { get; private set; }
    public RiskLevel Level { get; private set; }

    private RiskLimit() { }

    public RiskLimit(
        Guid id,
        Guid portfolioId,
        RiskLimitType type,
        decimal threshold) : base(id)
    {
        PortfolioId = portfolioId;
        Type = type;
        Threshold = threshold;
        CurrentValue = 0m;
        IsBreached = false;
        Level = RiskLevel.Normal;
    }

    public void UpdateValue(decimal newValue)
    {
        CurrentValue = newValue;
        IsBreached = Math.Abs(newValue) > Math.Abs(Threshold);
        Level = CalculateLevel();
    }

    private RiskLevel CalculateLevel()
    {
        var ratio = Math.Abs(Threshold) > 0 ? Math.Abs(CurrentValue) / Math.Abs(Threshold) : 0m;
        return ratio switch
        {
            >= 1.0m => RiskLevel.Critical,
            >= 0.8m => RiskLevel.Warning,
            >= 0.6m => RiskLevel.Elevated,
            _ => RiskLevel.Normal
        };
    }
}
