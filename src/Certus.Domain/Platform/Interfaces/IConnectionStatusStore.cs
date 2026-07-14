using Certus.Domain.Platform.ValueObjects;

namespace Certus.Domain.Platform.Interfaces;

public interface IConnectionStatusStore
{
    ConnectionStatus Get(Guid connectionId);
    void Set(Guid connectionId, ConnectionStatus status);
    void Remove(Guid connectionId);
}
