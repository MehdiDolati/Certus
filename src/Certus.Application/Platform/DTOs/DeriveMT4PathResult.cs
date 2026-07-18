namespace Certus.Application.Platform.DTOs;

public record DeriveMT4PathResult
{
    public bool Success { get; init; }
    public string? MT4DataPath { get; init; }
    public string? ExpertsPath { get; init; }
    public bool IsCommonPath { get; init; }
    public string? CommonFilesPath { get; init; }
    public List<TerminalCandidate> TerminalCandidates { get; init; } = new();
    public string? ErrorMessage { get; init; }
}

public record TerminalCandidate
{
    public string MT4DataPath { get; init; } = string.Empty;
    public string ExpertsPath { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
}
