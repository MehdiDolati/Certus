using Certus.Domain.Platform.Enums;
using Certus.Domain.Platform.ValueObjects;
using FluentAssertions;

namespace Certus.Domain.Tests.Platform;

public class ConnectionStatusTests
{
    [Fact]
    public void Default_Should_Be_Disconnected()
    {
        var status = new ConnectionStatus();

        status.State.Should().Be(PlatformConnectionStatus.Disconnected);
        status.ConnectedAt.Should().BeNull();
        status.LastDataReceivedAt.Should().BeNull();
        status.ErrorMessage.Should().BeNull();
        status.RetryCount.Should().Be(0);
    }

    [Fact]
    public void Disconnected_Static_Should_Return_Disconnected_State()
    {
        ConnectionStatus.Disconnected.State.Should().Be(PlatformConnectionStatus.Disconnected);
    }

    [Fact]
    public void WithConnected_Should_Set_State_To_Connected()
    {
        var status = new ConnectionStatus();

        var connected = status.WithConnected();

        connected.State.Should().Be(PlatformConnectionStatus.Connected);
        connected.ConnectedAt.Should().NotBeNull();
        connected.ConnectedAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        connected.ErrorMessage.Should().BeNull();
        connected.RetryCount.Should().Be(0);
    }

    [Fact]
    public void WithDisconnected_Should_Set_State_And_Reason()
    {
        var status = new ConnectionStatus().WithConnected();

        var disconnected = status.WithDisconnected("User requested");

        disconnected.State.Should().Be(PlatformConnectionStatus.Disconnected);
        disconnected.ErrorMessage.Should().Be("User requested");
    }

    [Fact]
    public void WithError_Should_Increment_RetryCount()
    {
        var status = new ConnectionStatus().WithConnected();

        var errored = status.WithError("Timeout");

        errored.State.Should().Be(PlatformConnectionStatus.Error);
        errored.ErrorMessage.Should().Be("Timeout");
        errored.RetryCount.Should().Be(1);
    }

    [Fact]
    public void WithError_Should_Accumulate_RetryCount()
    {
        var status = new ConnectionStatus()
            .WithConnected()
            .WithError("Error 1")
            .WithError("Error 2")
            .WithError("Error 3");

        status.RetryCount.Should().Be(3);
    }

    [Fact]
    public void WithLastDataReceived_Should_Set_Timestamp()
    {
        var status = new ConnectionStatus().WithConnected();

        var updated = status.WithLastDataReceived();

        updated.LastDataReceivedAt.Should().NotBeNull();
        updated.LastDataReceivedAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Record_Equality_Should_Work()
    {
        var a = new ConnectionStatus { State = PlatformConnectionStatus.Connected, RetryCount = 1 };
        var b = new ConnectionStatus { State = PlatformConnectionStatus.Connected, RetryCount = 1 };

        a.Should().Be(b);
    }

    [Fact]
    public void Record_Inequality_Should_Work()
    {
        var a = new ConnectionStatus { State = PlatformConnectionStatus.Connected };
        var b = new ConnectionStatus { State = PlatformConnectionStatus.Disconnected };

        a.Should().NotBe(b);
    }
}
