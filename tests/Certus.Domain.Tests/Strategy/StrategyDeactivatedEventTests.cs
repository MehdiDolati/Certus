using Certus.Domain.Strategy.Events;
using Certus.Domain.SharedKernel;
using FluentAssertions;

namespace Certus.Domain.Tests.Strategy;

public class StrategyDeactivatedEventTests
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
        var @event = new StrategyDeactivated
        {
            StrategyId = strategyId,
            Reason = "Paused"
        };

        @event.StrategyId.Should().Be(strategyId);
    }

    [Fact]
    public void Should_Have_Reason()
    {
        var @event = new StrategyDeactivated
        {
            StrategyId = Guid.NewGuid(),
            Reason = "Retired"
        };

        @event.Reason.Should().Be("Retired");
    }

    [Fact]
    public void Record_Should_Be_Equal_When_Values_Are_Same()
    {
        var eventId = Guid.NewGuid();
        var strategyId = Guid.NewGuid();
        var occurredOn = DateTime.UtcNow;

        var event1 = new StrategyDeactivated
        {
            EventId = eventId,
            OccurredOn = occurredOn,
            StrategyId = strategyId,
            Reason = "Paused"
        };
        var event2 = new StrategyDeactivated
        {
            EventId = eventId,
            OccurredOn = occurredOn,
            StrategyId = strategyId,
            Reason = "Paused"
        };

        event1.Should().Be(event2);
    }

    private static StrategyDeactivated CreateEvent()
    {
        return new StrategyDeactivated
        {
            StrategyId = Guid.NewGuid(),
            Reason = "Paused"
        };
    }
}
