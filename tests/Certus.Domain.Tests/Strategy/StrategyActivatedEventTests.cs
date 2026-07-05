using Certus.Domain.Strategy.Events;
using Certus.Domain.Strategy.ValueObjects;
using Certus.Domain.SharedKernel;
using FluentAssertions;

namespace Certus.Domain.Tests.Strategy;

public class StrategyActivatedEventTests
{
    [Fact]
    public void Should_Be_DomainEvent()
    {
        var @event = CreateEvent();

        @event.Should().BeAssignableTo<IDomainEvent>();
    }

    [Fact]
    public void Should_Have_EventId()
    {
        var @event = CreateEvent();

        @event.EventId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Should_Have_OccurredOn()
    {
        var @event = CreateEvent();

        @event.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Should_Have_StrategyId()
    {
        var strategyId = Guid.NewGuid();
        var @event = new StrategyActivated
        {
            StrategyId = strategyId,
            Name = "Momentum",
            Type = new StrategyType(StrategyCategory.Momentum)
        };

        @event.StrategyId.Should().Be(strategyId);
    }

    [Fact]
    public void Should_Have_Name()
    {
        var @event = new StrategyActivated
        {
            StrategyId = Guid.NewGuid(),
            Name = "MeanReversion",
            Type = new StrategyType(StrategyCategory.MeanReversion)
        };

        @event.Name.Should().Be("MeanReversion");
    }

    [Fact]
    public void Should_Have_Type()
    {
        var type = new StrategyType(StrategyCategory.Arbitrage, "StatArb");
        var @event = new StrategyActivated
        {
            StrategyId = Guid.NewGuid(),
            Name = "Test",
            Type = type
        };

        @event.Type.Should().Be(type);
    }

    private static StrategyActivated CreateEvent()
    {
        return new StrategyActivated
        {
            StrategyId = Guid.NewGuid(),
            Name = "Test",
            Type = new StrategyType(StrategyCategory.Momentum)
        };
    }
}
