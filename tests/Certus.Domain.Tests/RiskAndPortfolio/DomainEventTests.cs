using Certus.Domain.RiskAndPortfolio.Enums;
using Certus.Domain.RiskAndPortfolio.Events;
using Certus.Domain.SharedKernel;
using FluentAssertions;

namespace Certus.Domain.Tests.RiskAndPortfolio;

public class DomainEventTests
{
    [Fact]
    public void PortfolioCreated_Should_Set_All_Properties()
    {
        var portfolioId = Guid.NewGuid();
        var capital = new Money(1_000_000m, Currency.USD);

        var evt = new PortfolioCreated
        {
            PortfolioId = portfolioId,
            Name = "Test Fund",
            AllocatedCapital = capital
        };

        evt.PortfolioId.Should().Be(portfolioId);
        evt.Name.Should().Be("Test Fund");
        evt.AllocatedCapital.Should().Be(capital);
        evt.EventId.Should().NotBe(Guid.Empty);
        evt.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void RiskAssessmentCompleted_Should_Set_All_Properties()
    {
        var portfolioId = Guid.NewGuid();

        var evt = new RiskAssessmentCompleted
        {
            PortfolioId = portfolioId,
            RiskLevel = RiskLevel.Warning,
            PortfolioVaR = 0.05m,
            CurrentDrawdown = 0.03m
        };

        evt.PortfolioId.Should().Be(portfolioId);
        evt.RiskLevel.Should().Be(RiskLevel.Warning);
        evt.PortfolioVaR.Should().Be(0.05m);
        evt.CurrentDrawdown.Should().Be(0.03m);
        evt.EventId.Should().NotBe(Guid.Empty);
        evt.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void RiskLimitAdjusted_Should_Set_All_Properties()
    {
        var portfolioId = Guid.NewGuid();

        var evt = new RiskLimitAdjusted
        {
            PortfolioId = portfolioId,
            LimitType = RiskLimitType.MaxDrawdown,
            OldThreshold = 0.1m,
            NewThreshold = 0.15m
        };

        evt.PortfolioId.Should().Be(portfolioId);
        evt.LimitType.Should().Be(RiskLimitType.MaxDrawdown);
        evt.OldThreshold.Should().Be(0.1m);
        evt.NewThreshold.Should().Be(0.15m);
        evt.EventId.Should().NotBe(Guid.Empty);
        evt.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void RiskLimitBreached_Should_Set_All_Properties()
    {
        var portfolioId = Guid.NewGuid();

        var evt = new RiskLimitBreached
        {
            PortfolioId = portfolioId,
            LimitType = RiskLimitType.MaxLeverage,
            Severity = RiskLevel.Critical,
            Threshold = 2.0m,
            CurrentValue = 2.5m
        };

        evt.PortfolioId.Should().Be(portfolioId);
        evt.LimitType.Should().Be(RiskLimitType.MaxLeverage);
        evt.Severity.Should().Be(RiskLevel.Critical);
        evt.Threshold.Should().Be(2.0m);
        evt.CurrentValue.Should().Be(2.5m);
        evt.EventId.Should().NotBe(Guid.Empty);
        evt.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void PortfolioCreated_Record_Equality()
    {
        var timestamp = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var eventId = Guid.NewGuid();
        var id = Guid.NewGuid();
        var capital = new Money(1_000_000m, Currency.USD);

        var a = new PortfolioCreated { EventId = eventId, OccurredOn = timestamp, PortfolioId = id, Name = "Fund", AllocatedCapital = capital };
        var b = new PortfolioCreated { EventId = eventId, OccurredOn = timestamp, PortfolioId = id, Name = "Fund", AllocatedCapital = capital };

        a.Should().Be(b);
    }

    [Fact]
    public void RiskAssessmentCompleted_Record_Equality()
    {
        var timestamp = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var eventId = Guid.NewGuid();
        var id = Guid.NewGuid();

        var a = new RiskAssessmentCompleted { EventId = eventId, OccurredOn = timestamp, PortfolioId = id, RiskLevel = RiskLevel.Normal };
        var b = new RiskAssessmentCompleted { EventId = eventId, OccurredOn = timestamp, PortfolioId = id, RiskLevel = RiskLevel.Normal };

        a.Should().Be(b);
    }

    [Fact]
    public void RiskLimitAdjusted_Record_Equality()
    {
        var timestamp = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var eventId = Guid.NewGuid();
        var id = Guid.NewGuid();

        var a = new RiskLimitAdjusted { EventId = eventId, OccurredOn = timestamp, PortfolioId = id, LimitType = RiskLimitType.VaR };
        var b = new RiskLimitAdjusted { EventId = eventId, OccurredOn = timestamp, PortfolioId = id, LimitType = RiskLimitType.VaR };

        a.Should().Be(b);
    }

    [Fact]
    public void RiskLimitBreached_Record_Equality()
    {
        var timestamp = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var eventId = Guid.NewGuid();
        var id = Guid.NewGuid();

        var a = new RiskLimitBreached { EventId = eventId, OccurredOn = timestamp, PortfolioId = id, LimitType = RiskLimitType.VaR, Severity = RiskLevel.Warning };
        var b = new RiskLimitBreached { EventId = eventId, OccurredOn = timestamp, PortfolioId = id, LimitType = RiskLimitType.VaR, Severity = RiskLevel.Warning };

        a.Should().Be(b);
    }

    [Fact]
    public void All_Events_Implement_IDomainEvent()
    {
        IDomainEvent portfolioCreated = new PortfolioCreated { PortfolioId = Guid.NewGuid(), Name = "F", AllocatedCapital = new Money(0, Currency.USD) };
        IDomainEvent assessmentCompleted = new RiskAssessmentCompleted { PortfolioId = Guid.NewGuid(), RiskLevel = RiskLevel.Normal };
        IDomainEvent limitAdjusted = new RiskLimitAdjusted { PortfolioId = Guid.NewGuid(), LimitType = RiskLimitType.VaR };
        IDomainEvent limitBreached = new RiskLimitBreached { PortfolioId = Guid.NewGuid(), LimitType = RiskLimitType.VaR, Severity = RiskLevel.Normal };

        portfolioCreated.Should().BeAssignableTo<IDomainEvent>();
        assessmentCompleted.Should().BeAssignableTo<IDomainEvent>();
        limitAdjusted.Should().BeAssignableTo<IDomainEvent>();
        limitBreached.Should().BeAssignableTo<IDomainEvent>();
    }
}
