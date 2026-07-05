using Certus.Domain.Platform.Aggregates;
using Certus.Domain.Platform.Enums;
using Certus.Domain.Platform.Events;
using Certus.Domain.Platform.ValueObjects;
using FluentAssertions;

namespace Certus.Domain.Tests.Platform;

public class PlatformConnectionTests
{
    [Fact]
    public void Create_Should_Return_Connection_With_Correct_Defaults()
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
        connection.Status.Should().Be(PlatformConnectionStatus.Disconnected);
        connection.Config.Should().Be(config);
        connection.ConnectedAt.Should().BeNull();
        connection.ErrorMessage.Should().BeNull();
        connection.RetryCount.Should().Be(0);
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

    [Fact]
    public void Connect_Should_Set_Status_To_Connected()
    {
        var connection = CreateConnection();

        connection.Connect();

        connection.Status.Should().Be(PlatformConnectionStatus.Connected);
        connection.ConnectedAt.Should().NotBeNull();
        connection.ConnectedAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        connection.DisconnectedAt.Should().BeNull();
        connection.ErrorMessage.Should().BeNull();
        connection.RetryCount.Should().Be(0);
    }

    [Fact]
    public void Connect_Should_Raise_PlatformConnected_Event()
    {
        var connection = CreateConnection();

        connection.Connect();

        connection.DomainEvents.Should().ContainSingle(e => e is PlatformConnected);
        var domainEvent = connection.DomainEvents.OfType<PlatformConnected>().First();
        domainEvent.ConnectionId.Should().Be(connection.Id);
        domainEvent.PlatformId.Should().Be("metatrader4");
    }

    [Fact]
    public void Disconnect_Should_Set_Status_To_Disconnected()
    {
        var connection = CreateConnection();
        connection.Connect();

        connection.Disconnect("User requested");

        connection.Status.Should().Be(PlatformConnectionStatus.Disconnected);
        connection.DisconnectedAt.Should().NotBeNull();
        connection.ErrorMessage.Should().Be("User requested");
    }

    [Fact]
    public void Disconnect_Should_Raise_PlatformDisconnected_Event()
    {
        var connection = CreateConnection();
        connection.Connect();

        connection.Disconnect("Test reason");

        connection.DomainEvents.Should().ContainSingle(e => e is PlatformDisconnected);
        var domainEvent = connection.DomainEvents.OfType<PlatformDisconnected>().First();
        domainEvent.Reason.Should().Be("Test reason");
    }

    [Fact]
    public void SetError_Should_Set_Status_To_Error()
    {
        var connection = CreateConnection();
        connection.Connect();

        connection.SetError("Connection timeout");

        connection.Status.Should().Be(PlatformConnectionStatus.Error);
        connection.ErrorMessage.Should().Be("Connection timeout");
        connection.RetryCount.Should().Be(1);
    }

    [Fact]
    public void SetError_Should_Increment_RetryCount()
    {
        var connection = CreateConnection();
        connection.Connect();

        connection.SetError("Error 1");
        connection.SetError("Error 2");
        connection.SetError("Error 3");

        connection.RetryCount.Should().Be(3);
    }

    [Fact]
    public void ShouldReconnect_Should_Return_True_When_RetryCount_Below_Max()
    {
        var connection = CreateConnection();
        connection.Connect();
        connection.SetError("Error");

        connection.ShouldReconnect(maxRetries: 5).Should().BeTrue();
    }

    [Fact]
    public void ShouldReconnect_Should_Return_False_When_RetryCount_Exceeds_Max()
    {
        var connection = CreateConnection();
        connection.Connect();
        
        for (int i = 0; i < 5; i++)
            connection.SetError($"Error {i}");

        connection.ShouldReconnect(maxRetries: 5).Should().BeFalse();
    }

    [Fact]
    public void ShouldReconnect_Should_Return_False_When_Not_In_Error_State()
    {
        var connection = CreateConnection();
        connection.Connect();

        connection.ShouldReconnect().Should().BeFalse();
    }

    [Fact]
    public void UpdateLastDataReceived_Should_Set_Timestamp()
    {
        var connection = CreateConnection();
        connection.Connect();

        connection.UpdateLastDataReceived();

        connection.LastDataReceivedAt.Should().NotBeNull();
        connection.LastDataReceivedAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    private static PlatformConnection CreateConnection()
    {
        var config = new PlatformConfig
        {
            PlatformType = PlatformType.MetaTrader4,
            DataFormat = DataFormat.Json,
            FilePath = @"C:\MT4\Data\portfolio.json"
        };

        return PlatformConnection.Create("metatrader4", "MetaTrader 4", config);
    }
}
