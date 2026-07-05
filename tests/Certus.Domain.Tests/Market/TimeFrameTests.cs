using Certus.Domain.Market.ValueObjects;
using FluentAssertions;

namespace Certus.Domain.Tests.Market;

public class TimeFrameTests
{
    [Fact]
    public void Constructor_Should_Set_Value()
    {
        var tf = new TimeFrame("4h");

        tf.Value.Should().Be("4h");
    }

    [Fact]
    public void ToString_Should_Return_Value()
    {
        var tf = new TimeFrame("1d");

        tf.ToString().Should().Be("1d");
    }

    [Fact]
    public void Static_TimeFrames_Should_Have_Correct_Values()
    {
        TimeFrame.OneMinute.Value.Should().Be("1m");
        TimeFrame.FiveMinutes.Value.Should().Be("5m");
        TimeFrame.FifteenMinutes.Value.Should().Be("15m");
        TimeFrame.OneHour.Value.Should().Be("1h");
        TimeFrame.FourHours.Value.Should().Be("4h");
        TimeFrame.OneDay.Value.Should().Be("1d");
        TimeFrame.OneWeek.Value.Should().Be("1w");
    }

    [Fact]
    public void Record_Equality_Should_Work()
    {
        var tf1 = new TimeFrame("1h");
        var tf2 = new TimeFrame("1h");

        tf1.Should().Be(tf2);
    }

    [Fact]
    public void Record_Inequality_Should_Work()
    {
        var tf1 = new TimeFrame("1h");
        var tf2 = new TimeFrame("4h");

        tf1.Should().NotBe(tf2);
    }

    [Fact]
    public void Static_TimeFrames_Should_Be_Distinct()
    {
        var frames = new[]
        {
            TimeFrame.OneMinute, TimeFrame.FiveMinutes, TimeFrame.FifteenMinutes,
            TimeFrame.OneHour, TimeFrame.FourHours, TimeFrame.OneDay, TimeFrame.OneWeek
        };

        frames.Distinct().Count().Should().Be(7);
    }
}
