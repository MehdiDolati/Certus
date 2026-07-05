using Certus.Domain.Strategy.ValueObjects;
using FluentAssertions;

namespace Certus.Domain.Tests.Strategy;

public class WeightTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(0.5)]
    [InlineData(1)]
    public void Constructor_Valid_Values_Should_Succeed(decimal value)
    {
        var weight = new Weight(value);

        weight.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void Constructor_Invalid_Values_Should_Throw(decimal value)
    {
        var act = () => new Weight(value);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Zero_Should_Have_Value_Zero()
    {
        Weight.Zero.Value.Should().Be(0m);
    }

    [Fact]
    public void Full_Should_Have_Value_One()
    {
        Weight.Full.Value.Should().Be(1m);
    }

    [Fact]
    public void ToString_Should_Return_Percentage()
    {
        var weight = new Weight(0.5m);

        weight.ToString().Should().Contain("50").And.Contain("%");
    }

    [Fact]
    public void ToString_Zero_Should_Return_Zero_Percent()
    {
        Weight.Zero.ToString().Should().Contain("0").And.Contain("%");
    }

    [Fact]
    public void ToString_Full_Should_Return_100_Percent()
    {
        Weight.Full.ToString().Should().Contain("100").And.Contain("%");
    }

    [Fact]
    public void Record_Should_Be_Equal_When_Values_Are_Same()
    {
        var weight1 = new Weight(0.5m);
        var weight2 = new Weight(0.5m);

        weight1.Should().Be(weight2);
    }

    [Fact]
    public void Record_Should_Be_Inequal_When_Values_Differ()
    {
        var weight1 = new Weight(0.5m);
        var weight2 = new Weight(0.3m);

        weight1.Should().NotBe(weight2);
    }
}
