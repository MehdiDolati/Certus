using Certus.Domain.Strategy.Enums;
using FluentAssertions;

namespace Certus.Domain.Tests.Strategy;

public class StrategyStatusTests
{
    [Theory]
    [InlineData(StrategyStatus.Draft, 0)]
    [InlineData(StrategyStatus.Backtesting, 1)]
    [InlineData(StrategyStatus.Active, 2)]
    [InlineData(StrategyStatus.Paused, 3)]
    [InlineData(StrategyStatus.Retired, 4)]
    public void Should_Have_Correct_Values(StrategyStatus status, int expected)
    {
        ((int)status).Should().Be(expected);
    }

    [Fact]
    public void Should_Have_Five_Values()
    {
        var values = Enum.GetValues<StrategyStatus>();

        values.Should().HaveCount(5);
    }
}
