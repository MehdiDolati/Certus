using System.Collections.Concurrent;
using Certus.Domain.Platform.Interfaces;
using Certus.Domain.Platform.ValueObjects;

namespace Certus.Infrastructure.Platform;

public class InMemoryConnectionStatusStore : IConnectionStatusStore
{
    private readonly ConcurrentDictionary<Guid, ConnectionStatus> _statuses = new();

    public ConnectionStatus Get(Guid connectionId)
    {
        return _statuses.TryGetValue(connectionId, out var status)
            ? status
            : ConnectionStatus.Disconnected;
    }

    public void Set(Guid connectionId, ConnectionStatus status)
    {
        _statuses[connectionId] = status;
    }

    public void Remove(Guid connectionId)
    {
        _statuses.TryRemove(connectionId, out _);
    }
}
