namespace Certus.Application.Platform.DTOs;

public record DeployPortfolioRequest
{
    public string SourceFolderPath { get; init; } = string.Empty;
    public Guid ConnectionId { get; init; }
    public string? PortfolioName { get; init; }
    public string? ManualMT4Path { get; init; }
    public string? SelectedTerminalPath { get; init; }
    public Dictionary<string, ConflictAction>? ConflictResolutions { get; init; }
}

public enum ConflictAction
{
    Skip,
    Overwrite
}
