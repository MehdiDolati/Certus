namespace Certus.Domain.Platform.Interfaces;

public interface IFileImportService : IDisposable
{
    void StartWatching(Guid connectionId, string filePath);
    void StartWatchingFile(Guid connectionId, string filePath, string watcherKey);
    void StartWatchingDirectory(Guid connectionId, string directory, string[] filePatterns);
    void StopWatching(Guid connectionId);
    bool IsWatching(Guid connectionId);
    DateTime? GetLastModifiedTime(Guid connectionId);
    Task<string?> ReadFileAsync(string filePath);
    event EventHandler<FileImportEventArgs>? FileChanged;
}

public class FileImportEventArgs : EventArgs
{
    public Guid ConnectionId { get; init; }
    public string FilePath { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}
