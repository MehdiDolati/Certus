using Certus.Domain.Platform.Enums;

namespace Certus.Application.Platform.DTOs;

public record ConnectPlatformRequest
{
    public string PlatformId { get; init; } = string.Empty;
    public string? Name { get; init; }
    public string? FilePath { get; init; }
    public string? ServerAddress { get; init; }
    public int? Port { get; init; }
    public string? ApiKey { get; init; }
    public bool UseFileWatcher { get; init; } = true;
}
