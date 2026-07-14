using Certus.Domain.Platform.Enums;

namespace Certus.Domain.Platform.ValueObjects;

public record ConnectionStatus
{
    public PlatformConnectionStatus State { get; init; } = PlatformConnectionStatus.Disconnected;
    public DateTime? ConnectedAt { get; init; }
    public DateTime? LastDataReceivedAt { get; init; }
    public string? ErrorMessage { get; init; }
    public int RetryCount { get; init; }

    public static ConnectionStatus Disconnected => new() { State = PlatformConnectionStatus.Disconnected };

    public ConnectionStatus WithConnected()
    {
        return this with
        {
            State = PlatformConnectionStatus.Connected,
            ConnectedAt = DateTime.UtcNow,
            ErrorMessage = null,
            RetryCount = 0
        };
    }

    public ConnectionStatus WithDisconnected(string? reason = null)
    {
        return this with
        {
            State = PlatformConnectionStatus.Disconnected,
            ErrorMessage = reason
        };
    }

    public ConnectionStatus WithError(string errorMessage)
    {
        return this with
        {
            State = PlatformConnectionStatus.Error,
            ErrorMessage = errorMessage,
            RetryCount = RetryCount + 1
        };
    }

    public ConnectionStatus WithLastDataReceived()
    {
        return this with { LastDataReceivedAt = DateTime.UtcNow };
    }
}
