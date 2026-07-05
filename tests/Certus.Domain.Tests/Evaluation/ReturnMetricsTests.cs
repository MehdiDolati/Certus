using Certus.Domain.Evaluation.ValueObjects;
using FluentAssertions;

namespace Certus.Domain.Tests.Evaluation;

public class ReturnMetricsTests
{
    [Fact]
    public void ReturnMetrics_Should_Store_All_Properties()
    {
        var monthlyReturns = new List<decimal> { 0.02m, 0.03m, -0.01m };
        var dailyReturns = new List<decimal> { 0.001m, -0.002m };

        var metrics = new ReturnMetrics(0.15m, 0.18m, monthlyReturns, dailyReturns);

        metrics.TotalReturn.Should().Be(0.15m);
        metrics.AnnualizedReturn.Should().Be(0.18m);
        metrics.MonthlyReturns.Should().BeEquivalentTo(monthlyReturns);
        metrics.DailyReturns.Should().BeEquivalentTo(dailyReturns);
    }

    [Fact]
    public void ReturnMetrics_Default_Constructor_Should_Have_Empty_Collections()
    {
        var metrics = new ReturnMetrics();

        metrics.TotalReturn.Should().Be(0m);
        metrics.AnnualizedReturn.Should().Be(0m);
        metrics.MonthlyReturns.Should().BeEmpty();
        metrics.DailyReturns.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0.1, true)]
    [InlineData(-0.1, false)]
    [InlineData(0.0, false)]
    public void IsPositive_Should_Return_Correctly(decimal totalReturn, bool expected)
    {
        var metrics = new ReturnMetrics(totalReturn, 0m, [], []);

        metrics.IsPositive.Should().Be(expected);
    }

    [Fact]
    public void BestMonth_Should_Return_Max()
    {
        var metrics = new ReturnMetrics(0.1m, 0.12m, [0.02m, 0.05m, -0.01m], []);

        metrics.BestMonth.Should().Be(0.05m);
    }

    [Fact]
    public void WorstMonth_Should_Return_Min()
    {
        var metrics = new ReturnMetrics(0.1m, 0.12m, [0.02m, 0.05m, -0.01m], []);

        metrics.WorstMonth.Should().Be(-0.01m);
    }

    [Fact]
    public void BestMonth_Should_Return_Zero_When_No_Monthly_Returns()
    {
        var metrics = new ReturnMetrics(0.1m, 0.12m, [], []);

        metrics.BestMonth.Should().Be(0m);
    }

    [Fact]
    public void WorstMonth_Should_Return_Zero_When_No_Monthly_Returns()
    {
        var metrics = new ReturnMetrics(0.1m, 0.12m, [], []);

        metrics.WorstMonth.Should().Be(0m);
    }
}
