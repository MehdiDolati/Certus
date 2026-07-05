using Certus.Domain.Strategy.Aggregates;
using Certus.Domain.Strategy.Enums;
using Certus.Domain.Strategy.ValueObjects;
using FluentAssertions;

namespace Certus.Domain.Tests;

public class StrategyTests
{
    [Fact]
    public void Strategy_Should_Be_Created_With_Correct_Defaults()
    {
        var id = Guid.NewGuid();
        var strategy = new StrategyDefinition(id, "Momentum", new StrategyType(StrategyCategory.Momentum), 0.25m, "Test");

        strategy.Id.Should().Be(id);
        strategy.Name.Should().Be("Momentum");
        strategy.TargetReturn.Should().Be(0.25m);
        strategy.Description.Should().Be("Test");
        strategy.Status.Should().Be(StrategyStatus.Draft);
    }

    [Fact]
    public void Strategy_IsActive_Should_Return_True_When_Status_Is_Active()
    {
        var strategy = CreateStrategy();
        strategy.Activate();
        strategy.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData(StrategyStatus.Paused)]
    [InlineData(StrategyStatus.Retired)]
    public void Strategy_IsActive_Should_Return_False_When_Status_Is_Not_Active(StrategyStatus status)
    {
        var strategy = CreateStrategy();
        strategy.Activate();
        if (status == StrategyStatus.Paused)
            strategy.Pause();
        else
            strategy.Retire();
        strategy.IsActive.Should().BeFalse();
    }

    private static StrategyDefinition CreateStrategy() =>
        new(Guid.NewGuid(), "Test", new StrategyType(StrategyCategory.Momentum), 0.2m);
}
