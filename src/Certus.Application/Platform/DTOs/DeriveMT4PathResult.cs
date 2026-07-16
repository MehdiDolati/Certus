namespace Certus.Application.Platform.DTOs;

public record DeriveMT4PathResult
{
    public bool Success { get; init; }
    public string? MT4DataPath { get; init; }
    public string? ExpertsPath { get; init; }
    public string? ErrorMessage { get; init; }
}
