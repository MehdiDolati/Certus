namespace Certus.Domain.RiskAndPortfolio.Enums;

public enum RiskLimitType
{
    MaxDrawdown = 0,
    MaxPositionSize = 1,
    MaxLeverage = 2,
    VaR = 3,
    Correlation = 4,
    Concentration = 5,
    DailyLossLimit = 6,
    MaxOpenTrades = 7
}
