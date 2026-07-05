using Certus.Domain.RiskAndPortfolio.Enums;
using FluentAssertions;

namespace Certus.Domain.Tests.RiskAndPortfolio;

public class EnumTests
{
    [Theory]
    [InlineData(RiskLimitType.MaxDrawdown, 0)]
    [InlineData(RiskLimitType.MaxPositionSize, 1)]
    [InlineData(RiskLimitType.MaxLeverage, 2)]
    [InlineData(RiskLimitType.VaR, 3)]
    [InlineData(RiskLimitType.Correlation, 4)]
    [InlineData(RiskLimitType.Concentration, 5)]
    [InlineData(RiskLimitType.DailyLossLimit, 6)]
    [InlineData(RiskLimitType.MaxOpenTrades, 7)]
    public void RiskLimitType_Should_Have_Correct_Values(RiskLimitType type, int expected)
    {
        ((int)type).Should().Be(expected);
    }

    [Theory]
    [InlineData(RiskLevel.Normal, 0)]
    [InlineData(RiskLevel.Elevated, 1)]
    [InlineData(RiskLevel.Warning, 2)]
    [InlineData(RiskLevel.Critical, 3)]
    public void RiskLevel_Should_Have_Correct_Values(RiskLevel level, int expected)
    {
        ((int)level).Should().Be(expected);
    }

    [Theory]
    [InlineData(MarketRegime.Normal, 0)]
    [InlineData(MarketRegime.Trending, 1)]
    [InlineData(MarketRegime.Ranging, 2)]
    [InlineData(MarketRegime.Volatile, 3)]
    [InlineData(MarketRegime.Crisis, 4)]
    public void MarketRegime_Should_Have_Correct_Values(MarketRegime regime, int expected)
    {
        ((int)regime).Should().Be(expected);
    }

    [Theory]
    [InlineData(PortfolioStatus.Draft, 0)]
    [InlineData(PortfolioStatus.Active, 1)]
    [InlineData(PortfolioStatus.Paused, 2)]
    [InlineData(PortfolioStatus.Closed, 3)]
    public void PortfolioStatus_Should_Have_Correct_Values(PortfolioStatus status, int expected)
    {
        ((int)status).Should().Be(expected);
    }
}
