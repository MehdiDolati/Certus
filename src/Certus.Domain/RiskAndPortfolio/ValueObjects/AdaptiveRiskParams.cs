using Certus.Domain.RiskAndPortfolio.Enums;
using Certus.Domain.SharedKernel;

namespace Certus.Domain.RiskAndPortfolio.ValueObjects;

public record AdaptiveRiskParams 
{
    public decimal VolatilityScalingFactor { get; }
    public decimal ConfidenceThreshold { get; }
    public MarketRegime MarketRegime { get; }
    public decimal MaxPositionMultiplier { get; }

    public AdaptiveRiskParams(
        decimal volatilityScalingFactor,
        decimal confidenceThreshold,
        MarketRegime marketRegime,
        decimal maxPositionMultiplier = 1.0m)
    {
        VolatilityScalingFactor = volatilityScalingFactor;
        ConfidenceThreshold = confidenceThreshold;
        MarketRegime = marketRegime;
        MaxPositionMultiplier = maxPositionMultiplier;
    }

    public static AdaptiveRiskParams Conservative => new(0.5m, 0.8m, MarketRegime.Normal);
    public static AdaptiveRiskParams Moderate => new(1.0m, 0.6m, MarketRegime.Normal);
    public static AdaptiveRiskParams Aggressive => new(1.5m, 0.4m, MarketRegime.Normal);
    public static AdaptiveRiskParams Crisis => new(0.3m, 0.9m, MarketRegime.Crisis, 0.5m);
}
