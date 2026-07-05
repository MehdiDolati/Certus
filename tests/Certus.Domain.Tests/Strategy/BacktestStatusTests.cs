using Certus.Domain.Strategy.Enums;
using FluentAssertions;

namespace Certus.Domain.Tests.Strategy;

public class BacktestStatusTests
{
    [Theory]
    [InlineData(BacktestStatus.Pending, 0)]
    [InlineData(BacktestStatus.Running, 1)]
    [InlineData(BacktestStatus.Completed, 2)]
    [InlineData(BacktestStatus.Failed, 3)]
    public void Should_Have_Correct_Values(BacktestStatus status, int expected)
    {
        ((int)status).Should().Be(expected);
    }

    [Fact]
    public void Should_Have_Four_Values()
    {
        var values = Enum.GetValues<BacktestStatus>();

        values.Should().HaveCount(4);
    }
}
