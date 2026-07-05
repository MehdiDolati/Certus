using Certus.Domain.SharedKernel;
using FluentAssertions;

namespace Certus.Domain.Tests.SharedKernel;

public class AggregateRootTests
{
    private class TestAggregate(Guid id) : AggregateRoot(id)
    {
        public void RaiseTestEvent(IDomainEvent domainEvent) => RaiseDomainEvent(domainEvent);
    }

    private record TestEvent : IDomainEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    }

    [Fact]
    public void DomainEvents_Should_Be_Empty_Initially()
    {
        var aggregate = new TestAggregate(Guid.NewGuid());
        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void RaiseDomainEvent_Should_Add_Event()
    {
        var aggregate = new TestAggregate(Guid.NewGuid());
        var @event = new TestEvent();
        aggregate.RaiseTestEvent(@event);
        aggregate.DomainEvents.Should().ContainSingle().Which.Should().Be(@event);
    }

    [Fact]
    public void ClearDomainEvents_Should_Remove_All()
    {
        var aggregate = new TestAggregate(Guid.NewGuid());
        aggregate.RaiseTestEvent(new TestEvent());
        aggregate.RaiseTestEvent(new TestEvent());
        aggregate.ClearDomainEvents();
        aggregate.DomainEvents.Should().BeEmpty();
    }
}
