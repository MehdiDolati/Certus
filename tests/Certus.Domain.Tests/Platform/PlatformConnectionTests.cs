using Certus.Domain.Platform.Aggregates;
using Certus.Domain.Platform.Enums;
using Certus.Domain.Platform.Events;
using Certus.Domain.Platform.ValueObjects;
using FluentAssertions;

namespace Certus.Domain.Tests.Platform;

public class PlatformConnectionTests
{
    [Fact]
    public void Create_Should_Return_Config_Only_Entity()
    {
        var config = new PlatformConfig
        {
            PlatformType = PlatformType.MetaTrader4,
            DataFormat = DataFormat.Json,
            FilePath = @"C:\MT4\Data\portfolio.json"
        };

        var connection = PlatformConnection.Create(
            "metatrader4",
            "MetaTrader 4",
            config);

        connection.Id.Should().NotBeEmpty();
        connection.PlatformId.Should().Be("metatrader4");
        connection.PlatformName.Should().Be("MetaTrader 4");
        connection.Config.Should().Be(config);
        connection.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_Should_Raise_PlatformConnectionCreated_Event()
    {
        var config = new PlatformConfig
        {
            PlatformType = PlatformType.MetaTrader4,
            DataFormat = DataFormat.Json
        };

        var connection = PlatformConnection.Create(
            "metatrader4",
            "MetaTrader 4",
            config);

        connection.DomainEvents.Should().HaveCount(1);
        connection.DomainEvents.Should().ContainSingle(e => e is PlatformConnectionCreated);

        var domainEvent = connection.DomainEvents.OfType<PlatformConnectionCreated>().First();
        domainEvent.ConnectionId.Should().Be(connection.Id);
        domainEvent.PlatformId.Should().Be("metatrader4");
    }
}
