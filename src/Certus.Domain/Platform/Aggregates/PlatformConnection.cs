using Certus.Domain.Platform.Events;
using Certus.Domain.Platform.ValueObjects;
using Certus.Domain.SharedKernel;

namespace Certus.Domain.Platform.Aggregates;

public class PlatformConnection : AggregateRoot
{
    public string PlatformId { get; private set; } = string.Empty;
    public string PlatformName { get; private set; } = string.Empty;
    public PlatformConfig Config { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }

    private PlatformConnection() { }

    public PlatformConnection(
        Guid id,
        string platformId,
        string platformName,
        PlatformConfig config) : base(id)
    {
        PlatformId = platformId;
        PlatformName = platformName;
        Config = config;
        CreatedAt = DateTime.UtcNow;
    }

    public static PlatformConnection Create(
        string platformId,
        string platformName,
        PlatformConfig config)
    {
        var connection = new PlatformConnection(
            Guid.NewGuid(),
            platformId,
            platformName,
            config);

        connection.RaiseDomainEvent(new PlatformConnectionCreated
        {
            ConnectionId = connection.Id,
            PlatformId = platformId
        });

        return connection;
    }
}
