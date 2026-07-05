using Certus.Domain.RiskAndPortfolio.Entities;
using Certus.Domain.SharedKernel;
using FluentAssertions;

namespace Certus.Domain.Tests.RiskAndPortfolio;

public class CapitalAllocationTests
{
    [Fact]
    public void Constructor_Should_Set_All_Properties()
    {
        var id = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var strategyId = Guid.NewGuid();
        var amount = new Money(500_000m, Currency.USD);

        var allocation = new CapitalAllocation(id, portfolioId, strategyId, amount, 0.5m);

        allocation.Id.Should().Be(id);
        allocation.PortfolioId.Should().Be(portfolioId);
        allocation.StrategyId.Should().Be(strategyId);
        allocation.AllocatedAmount.Should().Be(amount);
        allocation.Weight.Should().Be(0.5m);
    }

    [Fact]
    public void Constructor_Should_Set_AllocatedAt_To_UtcNow()
    {
        var before = DateTime.UtcNow;

        var allocation = new CapitalAllocation(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new Money(100m, Currency.USD), 0.1m);

        var after = DateTime.UtcNow;

        allocation.AllocatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void UpdateAllocation_Should_Update_Amount_And_Weight()
    {
        var allocation = new CapitalAllocation(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new Money(100m, Currency.USD), 0.1m);

        allocation.UpdateAllocation(new Money(200m, Currency.EUR), 0.3m);

        allocation.AllocatedAmount.Should().Be(new Money(200m, Currency.EUR));
        allocation.Weight.Should().Be(0.3m);
    }

    [Fact]
    public void Equals_SameId_Should_Be_Equal()
    {
        var id = Guid.NewGuid();
        var a = new CapitalAllocation(id, Guid.NewGuid(), Guid.NewGuid(), new Money(100m, Currency.USD), 0.1m);
        var b = new CapitalAllocation(id, Guid.NewGuid(), Guid.NewGuid(), new Money(999m, Currency.EUR), 0.9m);

        a.Should().Be(b);
    }

    [Fact]
    public void Equals_DifferentId_Should_Not_Be_Equal()
    {
        var a = new CapitalAllocation(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new Money(100m, Currency.USD), 0.1m);
        var b = new CapitalAllocation(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new Money(100m, Currency.USD), 0.1m);

        a.Should().NotBe(b);
    }
}
