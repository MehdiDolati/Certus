using Certus.Domain.Execution.ValueObjects;
using FluentAssertions;

namespace Certus.Domain.Tests.Execution;

public class SlippageMetricsTests
{
    [Fact]
    public void Constructor_Should_Calculate_Slippage_Percent()
    {
        var metrics = new SlippageMetrics(100m, 101m, 10m);

        // |101 - 100| / 100 * 100 = 1%
        metrics.ExpectedPrice.Should().Be(100m);
        metrics.ActualPrice.Should().Be(101m);
        metrics.SlippagePercent.Should().Be(1m);
    }

    [Fact]
    public void Constructor_Should_Calculate_Slippage_Amount()
    {
        var metrics = new SlippageMetrics(100m, 101m, 10m);

        // |101 - 100| * 10 = 10
        metrics.SlippageAmount.Should().Be(10m);
    }

    [Fact]
    public void Constructor_Should_Handle_Actual_Price_Lower_Than_Expected()
    {
        var metrics = new SlippageMetrics(100m, 99m, 10m);

        metrics.SlippagePercent.Should().Be(1m);
        metrics.SlippageAmount.Should().Be(10m);
    }

    [Fact]
    public void Constructor_Should_Handle_Zero_Expected_Price()
    {
        var metrics = new SlippageMetrics(0m, 5m, 10m);

        metrics.SlippagePercent.Should().Be(0m);
        metrics.SlippageAmount.Should().Be(50m);
    }

    [Fact]
    public void IsAcceptable_Should_Be_True_When_Below_0_5_Percent()
    {
        var metrics = new SlippageMetrics(100m, 100.4m, 10m);

        metrics.IsAcceptable.Should().BeTrue();
    }

    [Fact]
    public void IsAcceptable_Should_Be_False_When_At_0_5_Percent()
    {
        var metrics = new SlippageMetrics(100m, 100.5m, 10m);

        metrics.IsAcceptable.Should().BeFalse();
    }

    [Fact]
    public void IsAcceptable_Should_Be_False_When_Above_0_5_Percent()
    {
        var metrics = new SlippageMetrics(100m, 101m, 10m);

        metrics.IsAcceptable.Should().BeFalse();
    }

    [Fact]
    public void IsSevere_Should_Be_True_When_Above_2_Percent()
    {
        var metrics = new SlippageMetrics(100m, 103m, 10m);

        metrics.IsSevere.Should().BeTrue();
    }

    [Fact]
    public void IsSevere_Should_Be_False_When_At_2_Percent()
    {
        var metrics = new SlippageMetrics(100m, 102m, 10m);

        metrics.IsSevere.Should().BeFalse();
    }

    [Fact]
    public void IsSevere_Should_Be_False_When_Below_2_Percent()
    {
        var metrics = new SlippageMetrics(100m, 101m, 10m);

        metrics.IsSevere.Should().BeFalse();
    }

    [Fact]
    public void SlippageMetrics_Should_Be_Equivalent_By_Value()
    {
        var m1 = new SlippageMetrics(100m, 101m, 10m);
        var m2 = new SlippageMetrics(100m, 101m, 10m);

        m1.Should().Be(m2);
    }
}
