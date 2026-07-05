using Certus.Domain.Strategy.ValueObjects;
using FluentAssertions;

namespace Certus.Domain.Tests.Strategy;

public class ConfidenceIntervalTests
{
    [Fact]
    public void Constructor_Should_Set_Properties_Correctly()
    {
        var interval = new ConfidenceInterval(5m, 15m, 0.95m);

        interval.Lower.Should().Be(5m);
        interval.Upper.Should().Be(15m);
        interval.ConfidenceLevel.Should().Be(0.95m);
    }

    [Fact]
    public void Constructor_Upper_Less_Than_Lower_Should_Throw()
    {
        var act = () => new ConfidenceInterval(15m, 5m, 0.95m);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Upper bound must be greater than lower bound*");
    }

    [Fact]
    public void Constructor_Upper_Equals_Lower_Should_Throw()
    {
        var act = () => new ConfidenceInterval(10m, 10m, 0.95m);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.5)]
    public void Constructor_ConfidenceLevel_Zero_Or_Negative_Should_Throw(decimal confidence)
    {
        var act = () => new ConfidenceInterval(5m, 15m, confidence);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(1.5)]
    public void Constructor_ConfidenceLevel_One_Or_Above_Should_Throw(decimal confidence)
    {
        var act = () => new ConfidenceInterval(5m, 15m, confidence);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Midpoint_Should_Return_Correct_Value()
    {
        var interval = new ConfidenceInterval(5m, 15m, 0.95m);

        interval.Midpoint.Should().Be(10m);
    }

    [Fact]
    public void Midpoint_Should_Work_With_Negative_Values()
    {
        var interval = new ConfidenceInterval(-10m, -5m, 0.95m);

        interval.Midpoint.Should().Be(-7.5m);
    }

    [Fact]
    public void Width_Should_Return_Correct_Value()
    {
        var interval = new ConfidenceInterval(5m, 15m, 0.95m);

        interval.Width.Should().Be(10m);
    }

    [Theory]
    [InlineData(5, true)]
    [InlineData(10, true)]
    [InlineData(15, true)]
    [InlineData(4, false)]
    [InlineData(16, false)]
    [InlineData(-1, false)]
    public void Contains_Should_Return_Correct_Value(decimal value, bool expected)
    {
        var interval = new ConfidenceInterval(5m, 15m, 0.95m);

        interval.Contains(value).Should().Be(expected);
    }

    [Fact]
    public void Contains_At_Lower_Bound_Should_Return_True()
    {
        var interval = new ConfidenceInterval(5m, 15m, 0.95m);

        interval.Contains(5m).Should().BeTrue();
    }

    [Fact]
    public void Contains_At_Upper_Bound_Should_Return_True()
    {
        var interval = new ConfidenceInterval(5m, 15m, 0.95m);

        interval.Contains(15m).Should().BeTrue();
    }

    [Fact]
    public void Record_Should_Be_Equal_When_Values_Are_Same()
    {
        var interval1 = new ConfidenceInterval(5m, 15m, 0.95m);
        var interval2 = new ConfidenceInterval(5m, 15m, 0.95m);

        interval1.Should().Be(interval2);
    }
}
