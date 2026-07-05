using Certus.Domain.Evaluation.ValueObjects;
using FluentAssertions;

namespace Certus.Domain.Tests.Evaluation;

public class RiskMetricsTests
{
    [Fact]
    public void RiskMetrics_Should_Store_All_Properties()
    {
        var metrics = new RiskMetrics(1.5m, 2.0m, 0.15m, 1.8m, 0.12m, 0.05m);

        metrics.SharpeRatio.Should().Be(1.5m);
        metrics.SortinoRatio.Should().Be(2.0m);
        metrics.MaxDrawdown.Should().Be(0.15m);
        metrics.CalmarRatio.Should().Be(1.8m);
        metrics.Volatility.Should().Be(0.12m);
        metrics.VaR.Should().Be(0.05m);
    }

    [Fact]
    public void IsHighQuality_Should_Return_True_When_Sharpe_High_And_Drawdown_Low()
    {
        var metrics = new RiskMetrics(1.5m, 2.0m, 0.15m, 1.8m, 0.12m, 0.05m);

        metrics.IsHighQuality.Should().BeTrue();
    }

    [Theory]
    [InlineData(0.5, 0.15, false)]
    [InlineData(1.5, 0.25, false)]
    [InlineData(0.5, 0.25, false)]
    public void IsHighQuality_Should_Return_False_When_Conditions_Not_Met(decimal sharpe, decimal maxDrawdown, bool expected)
    {
        var metrics = new RiskMetrics(sharpe, 1.0m, maxDrawdown, 1.0m, 0.1m, 0.05m);

        metrics.IsHighQuality.Should().Be(expected);
    }

    [Fact]
    public void IsHighQuality_Should_Return_True_At_Boundary()
    {
        var metrics = new RiskMetrics(1.001m, 1.0m, 0.199m, 1.0m, 0.1m, 0.05m);

        metrics.IsHighQuality.Should().BeTrue();
    }
}
