using Certus.Domain.Platform.Enums;
using Certus.Domain.Platform.Events;
using Certus.Domain.Platform.ValueObjects;
using Certus.Domain.SharedKernel;

namespace Certus.Domain.Platform.Aggregates;

public class PlatformConnection : AggregateRoot
{
    public string PlatformId { get; private set; } = string.Empty;
    public string PlatformName { get; private set; } = string.Empty;
    public PlatformConnectionStatus Status { get; private set; }
    public DateTime? ConnectedAt { get; private set; }
    public DateTime? DisconnectedAt { get; private set; }
    public DateTime? LastDataReceivedAt { get; private set; }
    public PlatformConfig Config { get; private set; } = null!;
    public string? ErrorMessage { get; private set; }
    public int RetryCount { get; private set; }

    private PlatformConnection() { }

    public PlatformConnection(
        Guid id,
        string platformId,
        string platformName,
        PlatformConnectionStatus status,
        PlatformConfig config) : base(id)
    {
        PlatformId = platformId;
        PlatformName = platformName;
        Status = status;
        Config = config;
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
            PlatformConnectionStatus.Disconnected,
            config);

        connection.RaiseDomainEvent(new PlatformConnectionCreated
        {
            ConnectionId = connection.Id,
            PlatformId = platformId
        });

        return connection;
    }

    public void Connect()
    {
        Status = PlatformConnectionStatus.Connected;
        ConnectedAt = DateTime.UtcNow;
        DisconnectedAt = null;
        ErrorMessage = null;
        RetryCount = 0;

        RaiseDomainEvent(new PlatformConnected
        {
            ConnectionId = Id,
            PlatformId = PlatformId
        });
    }

    public void Disconnect(string? reason = null)
    {
        Status = PlatformConnectionStatus.Disconnected;
        DisconnectedAt = DateTime.UtcNow;
        ErrorMessage = reason;

        RaiseDomainEvent(new PlatformDisconnected
        {
            ConnectionId = Id,
            PlatformId = PlatformId,
            Reason = reason
        });
    }

    public void SetError(string errorMessage)
    {
        Status = PlatformConnectionStatus.Error;
        ErrorMessage = errorMessage;
        RetryCount++;

        RaiseDomainEvent(new PlatformConnectionError
        {
            ConnectionId = Id,
            PlatformId = PlatformId,
            Error = errorMessage
        });
    }

    public void StartReconnecting()
    {
        Status = PlatformConnectionStatus.Reconnecting;
    }

    public void UpdateLastDataReceived()
    {
        LastDataReceivedAt = DateTime.UtcNow;
    }

    public bool ShouldReconnect(int maxRetries = 5)
    {
        return RetryCount < maxRetries && 
               Status == PlatformConnectionStatus.Error;
    }
}
