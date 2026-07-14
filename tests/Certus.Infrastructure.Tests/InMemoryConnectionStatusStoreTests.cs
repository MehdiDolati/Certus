using Certus.Domain.Platform.Enums;
using Certus.Domain.Platform.ValueObjects;
using Certus.Infrastructure.Platform;
using FluentAssertions;

namespace Certus.Infrastructure.Tests;

public class InMemoryConnectionStatusStoreTests
{
    private readonly InMemoryConnectionStatusStore _store = new();

    [Fact]
    public void Get_Should_Return_Disconnected_When_Not_Set()
    {
        var status = _store.Get(Guid.NewGuid());

        status.State.Should().Be(PlatformConnectionStatus.Disconnected);
    }

    [Fact]
    public void Set_Should_Store_Status()
    {
        var id = Guid.NewGuid();
        var status = ConnectionStatus.Disconnected.WithConnected();

        _store.Set(id, status);

        _store.Get(id).State.Should().Be(PlatformConnectionStatus.Connected);
    }

    [Fact]
    public void Set_Should_Overwrite_Previous_Status()
    {
        var id = Guid.NewGuid();
        _store.Set(id, ConnectionStatus.Disconnected.WithConnected());
        _store.Set(id, ConnectionStatus.Disconnected.WithDisconnected("test"));

        _store.Get(id).State.Should().Be(PlatformConnectionStatus.Disconnected);
        _store.Get(id).ErrorMessage.Should().Be("test");
    }

    [Fact]
    public void Remove_Should_Clear_Status()
    {
        var id = Guid.NewGuid();
        _store.Set(id, ConnectionStatus.Disconnected.WithConnected());

        _store.Remove(id);

        _store.Get(id).State.Should().Be(PlatformConnectionStatus.Disconnected);
    }

    [Fact]
    public void Remove_Should_Be_Noop_For_Nonexistent_Id()
    {
        var act = () => _store.Remove(Guid.NewGuid());
        act.Should().NotThrow();
    }
}
