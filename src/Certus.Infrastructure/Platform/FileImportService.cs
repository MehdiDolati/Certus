using Certus.Domain.Platform.Interfaces;

namespace Certus.Infrastructure.Platform;

public class FileImportService : IFileImportService
{
    private readonly Dictionary<string, FileSystemWatcher> _watchers = new();
    private readonly Dictionary<string, DateTime> _lastModified = new();
    private readonly Dictionary<string, string> _watchedPaths = new();
    private readonly Dictionary<string, string[]> _watchedPatterns = new();
    private readonly object _lock = new();
    private bool _disposed;

    public event EventHandler<FileImportEventArgs>? FileChanged;

    public void StartWatching(Guid connectionId, string filePath)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(FileImportService));

        string directory;
        string filter;

        if (Directory.Exists(filePath))
        {
            directory = filePath;
            filter = "portfolio_status.json";
        }
        else if (File.Exists(filePath))
        {
            directory = Path.GetDirectoryName(filePath) ?? filePath;
            filter = Path.GetFileName(filePath);
        }
        else
        {
            directory = Path.GetDirectoryName(filePath) ?? filePath;
            filter = Path.GetFileName(filePath);
        }

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
                Filter = filter,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.CreationTime,
                EnableRaisingEvents = true
            };

            watcher.Changed += OnFileChanged;
            watcher.Created += OnFileCreated;
            watcher.Error += OnWatcherError;

            _watchers[connectionId.ToString()] = watcher;
            _lastModified[connectionId.ToString()] = DateTime.MinValue;
            _watchedPaths[connectionId.ToString()] = Path.Combine(directory, filter);
        }
    }

    public void StartWatchingDirectory(Guid connectionId, string directory, string[] filePatterns)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(FileImportService));

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
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.CreationTime,
                EnableRaisingEvents = true
            };

            // Watch all files in directory, we'll filter in the handler
            watcher.Filter = "*.*";
            watcher.IncludeSubdirectories = false;

            watcher.Changed += OnDirectoryFileChanged;
            watcher.Created += OnDirectoryFileCreated;
            watcher.Error += OnWatcherError;

            _watchers[connectionId.ToString()] = watcher;
            _lastModified[connectionId.ToString()] = DateTime.MinValue;
            _watchedPaths[connectionId.ToString()] = directory;
            _watchedPatterns[connectionId.ToString()] = filePatterns.Select(p => p.ToLowerInvariant()).ToArray();
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
                watcher.Changed -= OnDirectoryFileChanged;
                watcher.Created -= OnDirectoryFileCreated;
                watcher.Error -= OnWatcherError;
                watcher.Dispose();
                _watchers.Remove(key);
            }

            _lastModified.Remove(key);
            _watchedPaths.Remove(key);
            _watchedPatterns.Remove(key);
        }
    }

    public bool IsWatching(Guid connectionId)
    {
        lock (_lock)
        {
            return _watchers.ContainsKey(connectionId.ToString());
        }
    }

    public DateTime? GetLastModifiedTime(Guid connectionId)
    {
        lock (_lock)
        {
            var key = connectionId.ToString();

            // If we've detected a change via the watcher, use that
            if (_lastModified.TryGetValue(key, out var lastMod) && lastMod != DateTime.MinValue)
                return lastMod;

            // Otherwise, fall back to the file's actual modification time
            if (_watchedPaths.TryGetValue(key, out var filePath) && File.Exists(filePath))
                return File.GetLastWriteTimeUtc(filePath);

            return null;
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

    private async void OnDirectoryFileChanged(object sender, FileSystemEventArgs e)
    {
        await Task.Delay(100);

        var connectionId = GetConnectionIdForPath(e.FullPath);
        if (connectionId == null) return;

        // Check if this file matches any of the watched patterns
        if (e.Name != null && !IsFileWatched(connectionId, e.Name))
            return;

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

    private async void OnDirectoryFileCreated(object sender, FileSystemEventArgs e)
    {
        await Task.Delay(100);

        var connectionId = GetConnectionIdForPath(e.FullPath);
        if (connectionId == null) return;

        // Check if this file matches any of the watched patterns
        if (e.Name != null && !IsFileWatched(connectionId, e.Name))
            return;

        await RaiseFileChangedAsync(connectionId, e.FullPath);
    }

    private bool IsFileWatched(string connectionId, string fileName)
    {
        lock (_lock)
        {
            if (!_watchedPatterns.TryGetValue(connectionId, out var patterns))
                return true; // No patterns = watch all (backwards compatible)

            return patterns.Any(p => fileName.EndsWith(p, StringComparison.OrdinalIgnoreCase));
        }
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
            _watchedPatterns.Clear();
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
