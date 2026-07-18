namespace Certus.Application.Platform.DTOs;

public record ValidateFolderResult
{
    public bool IsValid { get; init; }
    public List<string> EANames { get; init; } = [];
    public int EACount { get; init; }
    public string? ErrorMessage { get; init; }
}
