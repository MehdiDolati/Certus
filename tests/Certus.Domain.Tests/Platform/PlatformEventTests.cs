using Certus.Domain.Platform.Events;
using Certus.Domain.SharedKernel;
using FluentAssertions;

namespace Certus.Domain.Tests.Platform;

public class PlatformEventTests
{
    [Fact]
    public void DataReceived_Should_Implement_IDomainEvent()
    {
        var @event = new DataReceived
        {
            ConnectionId = Guid.NewGuid(),
            DataType = "portfolio_status",
            RecordCount = 5
        };

        @event.Should().BeAssignableTo<IDomainEvent>();
        @event.EventId.Should().NotBeEmpty();
        @event.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        @event.ConnectionId.Should().NotBeEmpty();
        @event.DataType.Should().Be("portfolio_status");
        @event.RecordCount.Should().Be(5);
    }

    [Fact]
    public void StrategyStatusChanged_Should_Implement_IDomainEvent()
    {
        var @event = new StrategyStatusChanged
        {
            StrategyId = Guid.NewGuid(),
            ExternalId = "EA_01",
            IsNowActive = true,
            WasActive = false
        };

        @event.Should().BeAssignableTo<IDomainEvent>();
        @event.EventId.Should().NotBeEmpty();
        @event.StrategyId.Should().NotBeEmpty();
        @event.ExternalId.Should().Be("EA_01");
        @event.IsNowActive.Should().BeTrue();
        @event.WasActive.Should().BeFalse();
    }

    [Fact]
    public void StrategyStatusChanged_Equality_Should_Be_By_All_Fields()
    {
        var id = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var occurredOn = DateTime.UtcNow;

        var left = new StrategyStatusChanged
        {
            EventId = eventId,
            OccurredOn = occurredOn,
            StrategyId = id,
            ExternalId = "EA_01",
            IsNowActive = true,
            WasActive = false
        };

        var right = new StrategyStatusChanged
        {
            EventId = eventId,
            OccurredOn = occurredOn,
            StrategyId = id,
            ExternalId = "EA_01",
            IsNowActive = true,
            WasActive = false
        };

        left.Should().Be(right);
    }
}
