using Certus.Domain.Platform.Entities;
using FluentAssertions;

namespace Certus.Domain.Tests.Platform;

public class StrategyMappingTests
{
    [Fact]
    public void Create_Should_Set_Properties_Correctly()
    {
        var connectionId = Guid.NewGuid();
        var strategyDefinitionId = Guid.NewGuid();

        var mapping = new StrategyMapping(
            Guid.NewGuid(),
            connectionId,
            "EA_Momentum_01",
            strategyDefinitionId);

        mapping.Id.Should().NotBeEmpty();
        mapping.ConnectionId.Should().Be(connectionId);
        mapping.StrategyExternalId.Should().Be("EA_Momentum_01");
        mapping.StrategyDefinitionId.Should().Be(strategyDefinitionId);
    }

    [Fact]
    public void Create_Should_Set_CreatedAt_To_UtcNow()
    {
        var mapping = new StrategyMapping(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "EA_01",
            Guid.NewGuid());

        mapping.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}
