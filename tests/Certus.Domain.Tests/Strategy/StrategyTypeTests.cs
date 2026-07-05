using Certus.Domain.Strategy.ValueObjects;
using FluentAssertions;

namespace Certus.Domain.Tests.Strategy;

public class StrategyTypeTests
{
    [Fact]
    public void Constructor_Should_Set_Category()
    {
        var type = new StrategyType(StrategyCategory.Momentum);

        type.Category.Should().Be(StrategyCategory.Momentum);
    }

    [Fact]
    public void Constructor_Should_Set_SubType()
    {
        var type = new StrategyType(StrategyCategory.Momentum, "RSI");

        type.SubType.Should().Be("RSI");
    }

    [Fact]
    public void Constructor_Default_SubType_Should_Be_Empty()
    {
        var type = new StrategyType(StrategyCategory.Momentum);

        type.SubType.Should().BeEmpty();
    }

    [Fact]
    public void ToString_Without_SubType_Should_Return_Category()
    {
        var type = new StrategyType(StrategyCategory.Momentum);

        type.ToString().Should().Be("Momentum");
    }

    [Fact]
    public void ToString_With_SubType_Should_Return_Category_SubType()
    {
        var type = new StrategyType(StrategyCategory.Momentum, "RSI");

        type.ToString().Should().Be("Momentum.RSI");
    }

    [Fact]
    public void ToString_With_Empty_SubType_Should_Return_Category()
    {
        var type = new StrategyType(StrategyCategory.Arbitrage, "");

        type.ToString().Should().Be("Arbitrage");
    }

    [Fact]
    public void Record_Should_Be_Equal_When_Values_Are_Same()
    {
        var type1 = new StrategyType(StrategyCategory.Momentum, "RSI");
        var type2 = new StrategyType(StrategyCategory.Momentum, "RSI");

        type1.Should().Be(type2);
    }

    [Fact]
    public void Record_Should_Be_Inequal_When_Values_Differ()
    {
        var type1 = new StrategyType(StrategyCategory.Momentum, "RSI");
        var type2 = new StrategyType(StrategyCategory.Momentum, "MACD");

        type1.Should().NotBe(type2);
    }
}
