using Certus.Domain.Platform.Enums;

namespace Certus.Application.Platform.DTOs;

public record PlatformConnectionDto
{
    public Guid Id { get; init; }
    public string PlatformId { get; init; } = string.Empty;
    public string PlatformName { get; init; } = string.Empty;
    public PlatformConnectionStatus Status { get; init; }
    public DateTime? ConnectedAt { get; init; }
    public DateTime? LastDataReceivedAt { get; init; }
    public string? ErrorMessage { get; init; }
    public int RetryCount { get; init; }
}
