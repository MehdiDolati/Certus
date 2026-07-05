using Certus.Domain.Strategy.Events;
using Certus.Domain.Strategy.ValueObjects;
using Certus.Domain.SharedKernel;
using FluentAssertions;

namespace Certus.Domain.Tests.Strategy;

public class BacktestCompletedEventTests
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
        var @event = new BacktestCompleted
        {
            StrategyId = strategyId,
            RunId = Guid.NewGuid(),
            Metrics = CreateMetrics()
        };

        @event.StrategyId.Should().Be(strategyId);
    }

    [Fact]
    public void Should_Have_RunId()
    {
        var runId = Guid.NewGuid();
        var @event = new BacktestCompleted
        {
            StrategyId = Guid.NewGuid(),
            RunId = runId,
            Metrics = CreateMetrics()
        };

        @event.RunId.Should().Be(runId);
    }

    [Fact]
    public void Should_Have_Metrics()
    {
        var metrics = CreateMetrics();
        var @event = new BacktestCompleted
        {
            StrategyId = Guid.NewGuid(),
            RunId = Guid.NewGuid(),
            Metrics = metrics
        };

        @event.Metrics.Should().Be(metrics);
    }

    [Fact]
    public void Record_Should_Be_Equal_When_Values_Are_Same()
    {
        var eventId = Guid.NewGuid();
        var strategyId = Guid.NewGuid();
        var runId = Guid.NewGuid();
        var occurredOn = DateTime.UtcNow;
        var metrics = CreateMetrics();

        var event1 = new BacktestCompleted
        {
            EventId = eventId,
            OccurredOn = occurredOn,
            StrategyId = strategyId,
            RunId = runId,
            Metrics = metrics
        };
        var event2 = new BacktestCompleted
        {
            EventId = eventId,
            OccurredOn = occurredOn,
            StrategyId = strategyId,
            RunId = runId,
            Metrics = metrics
        };

        event1.Should().Be(event2);
    }

    private static BacktestCompleted CreateEvent()
    {
        return new BacktestCompleted
        {
            StrategyId = Guid.NewGuid(),
            RunId = Guid.NewGuid(),
            Metrics = CreateMetrics()
        };
    }

    private static BacktestMetrics CreateMetrics()
    {
        return new BacktestMetrics(
            0.15m, 1.5m, 0.1m, 0.6m, 2.0m, 1.2m, 1.8m, 100, 2.5m);
    }
}
