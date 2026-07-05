namespace Certus.Domain.Platform.Interfaces;

public interface IFileImportService : IDisposable
{
    void StartWatching(Guid connectionId, string filePath);
    void StopWatching(Guid connectionId);
    bool IsWatching(Guid connectionId);
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
