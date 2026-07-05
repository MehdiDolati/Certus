using Certus.Domain.Market.ValueObjects;
using FluentAssertions;

namespace Certus.Domain.Tests.Market;

public class PriceRangeTests
{
    [Fact]
    public void Constructor_Should_Set_Low_And_High()
    {
        var range = new PriceRange(100m, 200m);

        range.Low.Should().Be(100m);
        range.High.Should().Be(200m);
    }

    [Fact]
    public void Spread_Should_Be_Difference_Between_High_And_Low()
    {
        var range = new PriceRange(100m, 200m);

        range.Spread.Should().Be(100m);
    }

    [Fact]
    public void Midpoint_Should_Be_Average_Of_Low_And_High()
    {
        var range = new PriceRange(100m, 200m);

        range.Midpoint.Should().Be(150m);
    }

    [Fact]
    public void Midpoint_Should_Be_Same_As_Value_When_Low_Equals_High()
    {
        var range = new PriceRange(50m, 50m);

        range.Midpoint.Should().Be(50m);
    }

    [Fact]
    public void Contains_Should_Return_True_For_Price_In_Range()
    {
        var range = new PriceRange(100m, 200m);

        range.Contains(150m).Should().BeTrue();
    }

    [Fact]
    public void Contains_Should_Return_True_For_Low_Boundary()
    {
        var range = new PriceRange(100m, 200m);

        range.Contains(100m).Should().BeTrue();
    }

    [Fact]
    public void Contains_Should_Return_True_For_High_Boundary()
    {
        var range = new PriceRange(100m, 200m);

        range.Contains(200m).Should().BeTrue();
    }

    [Fact]
    public void Contains_Should_Return_False_For_Price_Below_Range()
    {
        var range = new PriceRange(100m, 200m);

        range.Contains(99m).Should().BeFalse();
    }

    [Fact]
    public void Contains_Should_Return_False_For_Price_Above_Range()
    {
        var range = new PriceRange(100m, 200m);

        range.Contains(201m).Should().BeFalse();
    }

    [Fact]
    public void Spread_Should_Be_Zero_When_Low_Equals_High()
    {
        var range = new PriceRange(50m, 50m);

        range.Spread.Should().Be(0m);
    }

    [Fact]
    public void Record_Equality_Should_Work()
    {
        var r1 = new PriceRange(100m, 200m);
        var r2 = new PriceRange(100m, 200m);

        r1.Should().Be(r2);
        r1.GetHashCode().Should().Be(r2.GetHashCode());
    }
}
