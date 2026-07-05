using Certus.Domain.Market.Enums;
using Certus.Domain.Market.ValueObjects;
using FluentAssertions;

namespace Certus.Domain.Tests.Market;

public class TechnicalIndicatorTests
{
    private static readonly DateTime Now = DateTime.UtcNow;

    [Fact]
    public void Constructor_Should_Set_All_Properties()
    {
        var parameters = new Dictionary<string, decimal> { ["period"] = 14 };
        var indicator = new TechnicalIndicator(IndicatorType.RSI, "RSI-14", 55.5m, Now, parameters);

        indicator.Type.Should().Be(IndicatorType.RSI);
        indicator.Name.Should().Be("RSI-14");
        indicator.Value.Should().Be(55.5m);
        indicator.CalculatedAt.Should().Be(Now);
        indicator.Parameters.Should().HaveCount(1);
        indicator.Parameters!["period"].Should().Be(14m);
    }

    [Fact]
    public void Constructor_Should_Allow_Null_Parameters()
    {
        var indicator = new TechnicalIndicator(IndicatorType.EMA, "EMA-20", 150m, Now);

        indicator.Parameters.Should().BeNull();
    }

    [Fact]
    public void IsOverbought_Should_Be_True_When_RSI_Above_70()
    {
        var indicator = new TechnicalIndicator(IndicatorType.RSI, "RSI", 75m, Now);

        indicator.IsOverbought.Should().BeTrue();
    }

    [Fact]
    public void IsOverbought_Should_Be_False_When_RSI_Below_70()
    {
        var indicator = new TechnicalIndicator(IndicatorType.RSI, "RSI", 65m, Now);

        indicator.IsOverbought.Should().BeFalse();
    }

    [Fact]
    public void IsOverbought_Should_Be_False_For_Non_RSI_Indicators()
    {
        var indicator = new TechnicalIndicator(IndicatorType.MACD, "MACD", 80m, Now);

        indicator.IsOverbought.Should().BeFalse();
    }

    [Fact]
    public void IsOversold_Should_Be_True_When_RSI_Below_30()
    {
        var indicator = new TechnicalIndicator(IndicatorType.RSI, "RSI", 25m, Now);

        indicator.IsOversold.Should().BeTrue();
    }

    [Fact]
    public void IsOversold_Should_Be_False_When_RSI_Above_30()
    {
        var indicator = new TechnicalIndicator(IndicatorType.RSI, "RSI", 35m, Now);

        indicator.IsOversold.Should().BeFalse();
    }

    [Fact]
    public void IsOversold_Should_Be_False_For_Non_RSI_Indicators()
    {
        var indicator = new TechnicalIndicator(IndicatorType.EMA, "EMA", 10m, Now);

        indicator.IsOversold.Should().BeFalse();
    }

    [Fact]
    public void IsOverbought_Should_Be_False_When_RSI_Equals_70()
    {
        var indicator = new TechnicalIndicator(IndicatorType.RSI, "RSI", 70m, Now);

        indicator.IsOverbought.Should().BeFalse();
    }

    [Fact]
    public void IsOversold_Should_Be_False_When_RSI_Equals_30()
    {
        var indicator = new TechnicalIndicator(IndicatorType.RSI, "RSI", 30m, Now);

        indicator.IsOversold.Should().BeFalse();
    }

    [Fact]
    public void Record_Equality_Should_Work()
    {
        var parameters = new Dictionary<string, decimal> { ["period"] = 14 };
        var i1 = new TechnicalIndicator(IndicatorType.RSI, "RSI", 55m, Now, parameters);
        var i2 = new TechnicalIndicator(IndicatorType.RSI, "RSI", 55m, Now, parameters);

        i1.Should().Be(i2);
    }
}
