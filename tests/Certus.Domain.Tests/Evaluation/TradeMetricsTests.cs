using Certus.Domain.Evaluation.ValueObjects;
using FluentAssertions;

namespace Certus.Domain.Tests.Evaluation;

public class TradeMetricsTests
{
    [Fact]
    public void TradeMetrics_Should_Store_All_Properties()
    {
        var metrics = new TradeMetrics(100, 0.6m, 2.0m, 500m, 250m, 150m);

        metrics.TotalTrades.Should().Be(100);
        metrics.WinRate.Should().Be(0.6m);
        metrics.ProfitFactor.Should().Be(2.0m);
        metrics.AvgWin.Should().Be(500m);
        metrics.AvgLoss.Should().Be(250m);
        metrics.Expectancy.Should().Be(150m);
    }

    [Theory]
    [InlineData(150, true)]
    [InlineData(0, false)]
    [InlineData(-50, false)]
    public void IsProfitable_Should_Return_Correctly(decimal expectancy, bool expected)
    {
        var metrics = new TradeMetrics(100, 0.5m, 1.0m, 100m, 50m, expectancy);

        metrics.IsProfitable.Should().Be(expected);
    }

    [Fact]
    public void IsConsistent_Should_Return_True_When_High_WinRate_And_ProfitFactor()
    {
        var metrics = new TradeMetrics(100, 0.65m, 2.0m, 100m, 50m, 50m);

        metrics.IsConsistent.Should().BeTrue();
    }

    [Theory]
    [InlineData(0.4, 2.0, false)]
    [InlineData(0.6, 1.0, false)]
    [InlineData(0.4, 1.0, false)]
    public void IsConsistent_Should_Return_False_When_Conditions_Not_Met(decimal winRate, decimal profitFactor, bool expected)
    {
        var metrics = new TradeMetrics(100, winRate, profitFactor, 100m, 50m, 50m);

        metrics.IsConsistent.Should().Be(expected);
    }

    [Fact]
    public void IsConsistent_Should_Return_True_At_Boundary()
    {
        var metrics = new TradeMetrics(100, 0.501m, 1.501m, 100m, 50m, 50m);

        metrics.IsConsistent.Should().BeTrue();
    }
}
