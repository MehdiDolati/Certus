namespace Certus.Application.Platform.DTOs;

public record CheckConflictsResult
{
    public List<string> ConflictingFiles { get; init; } = [];
    public int TotalFiles { get; init; }
    public int NewFiles { get; init; }
}
