using Certus.Domain.Platform.Interfaces;

namespace Certus.Infrastructure.Platform;

public class FileImportService : IFileImportService
{
    private readonly Dictionary<string, FileSystemWatcher> _watchers = new();
    private readonly Dictionary<string, DateTime> _lastModified = new();
    private readonly object _lock = new();
    private bool _disposed;

    public event EventHandler<FileImportEventArgs>? FileChanged;

    public void StartWatching(Guid connectionId, string filePath)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(FileImportService));

        var directory = Path.GetDirectoryName(filePath);
        var fileName = Path.GetFileName(filePath);

        if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(fileName))
            throw new ArgumentException($"Invalid file path: {filePath}");

        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        lock (_lock)
        {
            if (_watchers.ContainsKey(connectionId.ToString()))
            {
                StopWatching(connectionId);
            }

            var watcher = new FileSystemWatcher
            {
                Path = directory,
                Filter = fileName,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.CreationTime,
                EnableRaisingEvents = true
            };

            watcher.Changed += OnFileChanged;
            watcher.Created += OnFileCreated;
            watcher.Error += OnWatcherError;

            _watchers[connectionId.ToString()] = watcher;
            _lastModified[connectionId.ToString()] = DateTime.MinValue;
        }
    }

    public void StopWatching(Guid connectionId)
    {
        lock (_lock)
        {
            var key = connectionId.ToString();
            if (_watchers.TryGetValue(key, out var watcher))
            {
                watcher.EnableRaisingEvents = false;
                watcher.Changed -= OnFileChanged;
                watcher.Created -= OnFileCreated;
                watcher.Error -= OnWatcherError;
                watcher.Dispose();
                _watchers.Remove(key);
            }

            _lastModified.Remove(key);
        }
    }

    public bool IsWatching(Guid connectionId)
    {
        lock (_lock)
        {
            return _watchers.ContainsKey(connectionId.ToString());
        }
    }

    public async Task<string?> ReadFileAsync(string filePath)
    {
        const int maxRetries = 3;
        const int baseDelayMs = 50;

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(stream);
                return await reader.ReadToEndAsync();
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            catch (IOException) when (i < maxRetries - 1)
            {
                await Task.Delay(baseDelayMs * (i + 1));
            }
            catch (IOException)
            {
                return null;
            }
        }

        return null;
    }

    private async void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        // Debounce: wait for file write to complete
        await Task.Delay(100);

        var connectionId = GetConnectionIdForPath(e.FullPath);
        if (connectionId == null) return;

        // Check if file was actually modified
        var lastMod = File.GetLastWriteTime(e.FullPath);
        lock (_lock)
        {
            if (_lastModified.TryGetValue(connectionId, out var lastKnown) && lastMod <= lastKnown)
                return;
            _lastModified[connectionId] = lastMod;
        }

        await RaiseFileChangedAsync(connectionId, e.FullPath);
    }

    private async void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        await Task.Delay(100);

        var connectionId = GetConnectionIdForPath(e.FullPath);
        if (connectionId == null) return;

        await RaiseFileChangedAsync(connectionId, e.FullPath);
    }

    private void OnWatcherError(object sender, ErrorEventArgs e)
    {
        // Log error but don't crash
        Console.WriteLine($"File watcher error: {e.GetException().Message}");
    }

    private async Task RaiseFileChangedAsync(string connectionId, string filePath)
    {
        try
        {
            var content = await ReadFileAsync(filePath);
            if (content != null)
            {
                FileChanged?.Invoke(this, new FileImportEventArgs
                {
                    ConnectionId = Guid.Parse(connectionId),
                    FilePath = filePath,
                    Content = content,
                    Timestamp = DateTime.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading file {filePath}: {ex.Message}");
        }
    }

    private string? GetConnectionIdForPath(string filePath)
    {
        lock (_lock)
        {
            return _watchers.FirstOrDefault(kvp => 
                filePath.Contains(Path.GetFileName(kvp.Value.Filter), StringComparison.OrdinalIgnoreCase)).Key;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        lock (_lock)
        {
            foreach (var watcher in _watchers.Values)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }
            _watchers.Clear();
            _lastModified.Clear();
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
