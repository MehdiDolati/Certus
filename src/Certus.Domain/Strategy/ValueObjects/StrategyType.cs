using Certus.Domain.SharedKernel;

namespace Certus.Domain.Strategy.ValueObjects;

public enum StrategyCategory
{
    Momentum = 0,
    MeanReversion = 1,
    Arbitrage = 2,
    TrendFollowing = 3,
    StatisticalArb = 4,
    MarketMaking = 5,
    Custom = 99
}

public record StrategyType 
{
    public StrategyCategory Category { get; }
    public string SubType { get; }

    public StrategyType(StrategyCategory category, string subType = "")
    {
        Category = category;
        SubType = subType;
    }

    public override string ToString() => string.IsNullOrEmpty(SubType)
        ? Category.ToString()
        : $"{Category}.{SubType}";
}
