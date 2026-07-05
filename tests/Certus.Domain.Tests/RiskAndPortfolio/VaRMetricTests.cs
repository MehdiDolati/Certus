using Certus.Domain.RiskAndPortfolio.ValueObjects;
using FluentAssertions;

namespace Certus.Domain.Tests.RiskAndPortfolio;

public class VaRMetricTests
{
    [Fact]
    public void Constructor_Should_Set_All_Properties()
    {
        var metric = new VaRMetric(50_000m, 0.95m, 10);

        metric.Value.Should().Be(50_000m);
        metric.ConfidenceLevel.Should().Be(0.95m);
        metric.TimeHorizonDays.Should().Be(10);
    }

    [Fact]
    public void DailyVaR_Should_Divide_By_Sqrt_TimeHorizon()
    {
        var metric = new VaRMetric(100m, 0.99m, 4);

        metric.DailyVaR.Should().Be(50m);
    }

    [Fact]
    public void DailyVaR_NonPerfectSquare_Should_Use_Sqrt()
    {
        var metric = new VaRMetric(100m, 0.99m, 2);

        var expected = 100m / (decimal)Math.Sqrt(2);

        metric.DailyVaR.Should().Be(expected);
    }

    [Fact]
    public void DailyVaR_ZeroDays_Should_Return_Value()
    {
        var metric = new VaRMetric(50m, 0.95m, 0);

        metric.DailyVaR.Should().Be(50m);
    }

    [Fact]
    public void DailyVaR_OneDay_Should_Return_Value()
    {
        var metric = new VaRMetric(50m, 0.95m, 1);

        metric.DailyVaR.Should().Be(50m);
    }

    [Fact]
    public void Record_Equality_SameValues_Should_Be_Equal()
    {
        var a = new VaRMetric(50_000m, 0.95m, 10);
        var b = new VaRMetric(50_000m, 0.95m, 10);

        a.Should().Be(b);
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void Record_Equality_DifferentValues_Should_Not_Be_Equal()
    {
        var a = new VaRMetric(50_000m, 0.95m, 10);
        var b = new VaRMetric(50_000m, 0.99m, 10);

        a.Should().NotBe(b);
        (a != b).Should().BeTrue();
    }
}
