using Certus.Domain.Platform.Enums;

namespace Certus.Domain.Platform.ValueObjects;

public record PlatformConfig
{
    public PlatformType PlatformType { get; init; }
    public DataFormat DataFormat { get; init; }
    public string? FilePath { get; init; }
    public string? ServerAddress { get; init; }
    public int? Port { get; init; }
    public string? ApiKey { get; init; }
    public int PollingIntervalMs { get; init; } = 5000;
    public bool UseFileWatcher { get; init; } = true;
    public int StaleThresholdSeconds { get; init; } = 10; // TODO: set to 300 for production
}

public record FilePlatformConfig : PlatformConfig
{
    public bool WatchForChanges { get; init; } = true;
    public int DebounceMs { get; init; } = 100;
}
