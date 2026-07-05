using Certus.Domain.Strategy.Entities;
using Certus.Domain.Strategy.ValueObjects;
using FluentAssertions;

namespace Certus.Domain.Tests.Strategy;

public class StrategySlotTests
{
    [Fact]
    public void Constructor_Should_Set_Properties_Correctly()
    {
        var id = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var weight = new Weight(0.3m);

        var slot = new StrategySlot(id, portfolioId, weight);

        slot.Id.Should().Be(id);
        slot.PortfolioId.Should().Be(portfolioId);
        slot.Weight.Should().Be(weight);
        slot.AssignedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void UpdateWeight_Should_Set_New_Weight()
    {
        var slot = CreateSlot(0.3m);
        var newWeight = new Weight(0.7m);

        slot.UpdateWeight(newWeight);

        slot.Weight.Should().Be(newWeight);
    }

    [Fact]
    public void UpdateWeight_To_Zero_Should_Succeed()
    {
        var slot = CreateSlot(0.5m);

        slot.UpdateWeight(Weight.Zero);

        slot.Weight.Value.Should().Be(0m);
    }

    private static StrategySlot CreateSlot(decimal weightValue)
    {
        return new StrategySlot(Guid.NewGuid(), Guid.NewGuid(), new Weight(weightValue));
    }
}
