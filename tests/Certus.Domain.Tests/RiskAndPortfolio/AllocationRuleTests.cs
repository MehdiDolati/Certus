using Certus.Domain.RiskAndPortfolio.Entities;
using FluentAssertions;

namespace Certus.Domain.Tests.RiskAndPortfolio;

public class AllocationRuleTests
{
    [Fact]
    public void Constructor_Should_Set_Properties_Correctly()
    {
        var id = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var strategyId = Guid.NewGuid();

        var rule = new AllocationRule(id, portfolioId, strategyId, 0.5m, 0.1m, 0.8m);

        rule.Id.Should().Be(id);
        rule.PortfolioId.Should().Be(portfolioId);
        rule.StrategyId.Should().Be(strategyId);
        rule.TargetWeight.Should().Be(0.5m);
        rule.MinWeight.Should().Be(0.1m);
        rule.MaxWeight.Should().Be(0.8m);
        rule.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Constructor_DefaultMinMax_Should_Use_Zero_One()
    {
        var rule = new AllocationRule(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 0.5m);

        rule.MinWeight.Should().Be(0m);
        rule.MaxWeight.Should().Be(1m);
    }

    [Fact]
    public void UpdateWeight_WithinRange_Should_Update()
    {
        var rule = new AllocationRule(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 0.5m, 0.1m, 0.8m);

        rule.UpdateWeight(0.3m, 0.1m, 0.9m);

        rule.TargetWeight.Should().Be(0.3m);
        rule.MinWeight.Should().Be(0.1m);
        rule.MaxWeight.Should().Be(0.9m);
    }

    [Fact]
    public void UpdateWeight_BelowMin_Should_Throw()
    {
        var rule = new AllocationRule(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 0.5m, 0.2m, 0.8m);

        var act = () => rule.UpdateWeight(0.1m, 0.2m, 0.8m);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Target weight must be within min/max range*");
    }

    [Fact]
    public void UpdateWeight_AboveMax_Should_Throw()
    {
        var rule = new AllocationRule(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 0.5m, 0.1m, 0.6m);

        var act = () => rule.UpdateWeight(0.8m, 0.1m, 0.6m);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Target weight must be within min/max range*");
    }

    [Fact]
    public void Deactivate_Should_Set_IsActive_False()
    {
        var rule = new AllocationRule(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 0.5m);

        rule.Deactivate();

        rule.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_Should_Set_IsActive_True()
    {
        var rule = new AllocationRule(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 0.5m);
        rule.Deactivate();

        rule.Activate();

        rule.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Equals_SameId_Should_Be_Equal()
    {
        var id = Guid.NewGuid();
        var a = new AllocationRule(id, Guid.NewGuid(), Guid.NewGuid(), 0.5m);
        var b = new AllocationRule(id, Guid.NewGuid(), Guid.NewGuid(), 0.8m);

        a.Should().Be(b);
    }

    [Fact]
    public void Equals_DifferentId_Should_Not_Be_Equal()
    {
        var a = new AllocationRule(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 0.5m);
        var b = new AllocationRule(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 0.5m);

        a.Should().NotBe(b);
    }
}
