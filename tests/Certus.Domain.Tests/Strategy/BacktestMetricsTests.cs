using Certus.Domain.Strategy.ValueObjects;
using FluentAssertions;

namespace Certus.Domain.Tests.Strategy;

public class BacktestMetricsTests
{
    [Fact]
    public void Constructor_Should_Set_All_Properties()
    {
        var metrics = new BacktestMetrics(
            0.15m, 1.5m, 0.1m, 0.6m, 2.0m, 1.2m, 1.8m, 100, 2.5m);

        metrics.TotalReturn.Should().Be(0.15m);
        metrics.SharpeRatio.Should().Be(1.5m);
        metrics.MaxDrawdown.Should().Be(0.1m);
        metrics.WinRate.Should().Be(0.6m);
        metrics.ProfitFactor.Should().Be(2.0m);
        metrics.CalmarRatio.Should().Be(1.2m);
        metrics.SortinoRatio.Should().Be(1.8m);
        metrics.TotalTrades.Should().Be(100);
        metrics.AvgTradeDuration.Should().Be(2.5m);
    }

    [Theory]
    [InlineData(0.01, true)]
    [InlineData(0.0, false)]
    [InlineData(-0.05, false)]
    public void IsProfitable_Should_Return_Correct_Value(decimal totalReturn, bool expected)
    {
        var metrics = CreateMetrics(totalReturn: totalReturn);

        metrics.IsProfitable.Should().Be(expected);
    }

    [Theory]
    [InlineData(1.5, 0.1, 0.6, true)]
    [InlineData(0.8, 0.1, 0.6, false)]
    [InlineData(1.5, 0.25, 0.6, false)]
    [InlineData(1.5, 0.1, 0.4, false)]
    [InlineData(0.5, 0.3, 0.4, false)]
    public void IsHighQuality_Should_Return_Correct_Value(
        decimal sharpe, decimal maxDrawdown, decimal winRate, bool expected)
    {
        var metrics = CreateMetrics(
            sharpeRatio: sharpe,
            maxDrawdown: maxDrawdown,
            winRate: winRate);

        metrics.IsHighQuality.Should().Be(expected);
    }

    [Fact]
    public void IsHighQuality_Should_Be_True_At_Boundary()
    {
        var metrics = new BacktestMetrics(
            totalReturn: 0.1m,
            sharpeRatio: 1.0001m,
            maxDrawdown: 0.1999m,
            winRate: 0.5001m,
            profitFactor: 1.5m,
            calmarRatio: 1.0m,
            sortinoRatio: 1.0m,
            totalTrades: 50,
            avgTradeDuration: 1.0m);

        metrics.IsHighQuality.Should().BeTrue();
    }

    [Fact]
    public void Record_Should_Be_Equal_When_Values_Are_Same()
    {
        var metrics1 = CreateMetrics();
        var metrics2 = CreateMetrics();

        metrics1.Should().Be(metrics2);
    }

    private static BacktestMetrics CreateMetrics(
        decimal totalReturn = 0.15m,
        decimal sharpeRatio = 1.5m,
        decimal maxDrawdown = 0.1m,
        decimal winRate = 0.6m)
    {
        return new BacktestMetrics(
            totalReturn, sharpeRatio, maxDrawdown, winRate,
            profitFactor: 2.0m, calmarRatio: 1.2m, sortinoRatio: 1.8m,
            totalTrades: 100, avgTradeDuration: 2.5m);
    }
}
