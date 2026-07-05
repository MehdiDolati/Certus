using Certus.Domain.Market.ValueObjects;
using FluentAssertions;

namespace Certus.Domain.Tests.Market;

public class PriceBarTests
{
    private static readonly DateTime Now = DateTime.UtcNow;

    [Fact]
    public void Constructor_Should_Set_All_Properties()
    {
        var bar = new PriceBar(Now, 100m, 110m, 95m, 105m, 5000m);

        bar.Timestamp.Should().Be(Now);
        bar.Open.Should().Be(100m);
        bar.High.Should().Be(110m);
        bar.Low.Should().Be(95m);
        bar.Close.Should().Be(105m);
        bar.Volume.Should().Be(5000m);
    }

    [Fact]
    public void TypicalPrice_Should_Be_Average_Of_High_Low_Close()
    {
        var bar = new PriceBar(Now, 100m, 120m, 80m, 90m, 1000m);

        // (120 + 80 + 90) / 3 = 96.666...
        bar.TypicalPrice.Should().BeApproximately(96.6667m, 0.001m);
    }

    [Fact]
    public void TypicalPrice_Should_Be_Close_When_All_Equal()
    {
        var bar = new PriceBar(Now, 50m, 50m, 50m, 50m, 100m);

        bar.TypicalPrice.Should().Be(50m);
    }

    [Fact]
    public void BodySize_Should_Be_Absolute_Difference_Between_Close_And_Open()
    {
        var bullishBar = new PriceBar(Now, 100m, 110m, 95m, 105m, 5000m);
        var bearishBar = new PriceBar(Now, 105m, 110m, 95m, 100m, 5000m);

        bullishBar.BodySize.Should().Be(5m);
        bearishBar.BodySize.Should().Be(5m);
    }

    [Fact]
    public void BodySize_Should_Be_Zero_When_Open_Equals_Close()
    {
        var bar = new PriceBar(Now, 100m, 110m, 90m, 100m, 5000m);

        bar.BodySize.Should().Be(0m);
    }

    [Fact]
    public void IsBullish_Should_Be_True_When_Close_Greater_Than_Open()
    {
        var bar = new PriceBar(Now, 100m, 110m, 95m, 105m, 5000m);

        bar.IsBullish.Should().BeTrue();
    }

    [Fact]
    public void IsBullish_Should_Be_False_When_Close_Less_Than_Open()
    {
        var bar = new PriceBar(Now, 105m, 110m, 95m, 100m, 5000m);

        bar.IsBullish.Should().BeFalse();
    }

    [Fact]
    public void IsBullish_Should_Be_False_When_Close_Equals_Open()
    {
        var bar = new PriceBar(Now, 100m, 110m, 95m, 100m, 5000m);

        bar.IsBullish.Should().BeFalse();
    }

    [Fact]
    public void Record_Equality_Should_Work_Correctly()
    {
        var bar1 = new PriceBar(Now, 100m, 110m, 95m, 105m, 5000m);
        var bar2 = new PriceBar(Now, 100m, 110m, 95m, 105m, 5000m);

        bar1.Should().Be(bar2);
        bar1.GetHashCode().Should().Be(bar2.GetHashCode());
    }

    [Fact]
    public void Record_Inequality_Should_Work_Correctly()
    {
        var bar1 = new PriceBar(Now, 100m, 110m, 95m, 105m, 5000m);
        var bar2 = new PriceBar(Now, 100m, 110m, 95m, 106m, 5000m);

        bar1.Should().NotBe(bar2);
    }
}
