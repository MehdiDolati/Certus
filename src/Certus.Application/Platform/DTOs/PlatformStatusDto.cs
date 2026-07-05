using Certus.Domain.Platform.Enums;

namespace Certus.Application.Platform.DTOs;

public record PlatformStatusDto
{
    public Guid ConnectionId { get; init; }
    public string PlatformName { get; init; } = string.Empty;
    public PlatformConnectionStatus Status { get; init; }
    public bool IsConnected { get; init; }
    public DateTime? ConnectedAt { get; init; }
    public DateTime? LastDataReceivedAt { get; init; }
    public int ImportedPortfolios { get; init; }
    public int ImportedStrategies { get; init; }
    public int ImportedTrades { get; init; }
    public string? ErrorMessage { get; init; }
}
