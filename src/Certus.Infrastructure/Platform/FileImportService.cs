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
            filter = "*.*";
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
            var watchedPath = filter == "*.*" ? directory : Path.Combine(directory, filter);
            _lastModified[watchedPath] = DateTime.MinValue;
            _watchedPaths[connectionId.ToString()] = watchedPath;
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

            if (_watchedPaths.TryGetValue(key, out var watchedPath))
                _lastModified.Remove(watchedPath);
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

            // Try to get last modified time from the watched path
            if (_watchedPaths.TryGetValue(key, out var watchedPath))
            {
                if (_lastModified.TryGetValue(watchedPath, out var lastMod) && lastMod != DateTime.MinValue)
                    return lastMod;

                // Fall back to the file's actual modification time
                if (File.Exists(watchedPath))
                    return File.GetLastWriteTimeUtc(watchedPath);
            }

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

        // Check if this specific file was actually modified (per-file, not per-connection)
        var lastMod = File.GetLastWriteTime(e.FullPath);
        lock (_lock)
        {
            if (_lastModified.TryGetValue(e.FullPath, out var lastKnown) && lastMod <= lastKnown)
                return;
            _lastModified[e.FullPath] = lastMod;
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

        // Check if this specific file was actually modified (per-file, not per-connection)
        var lastMod = File.GetLastWriteTime(e.FullPath);
        lock (_lock)
        {
            if (_lastModified.TryGetValue(e.FullPath, out var lastKnown) && lastMod <= lastKnown)
                return;
            _lastModified[e.FullPath] = lastMod;
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
            Console.WriteLine($"[FileImportService] Error reading file: {ex.Message}");
        }
    }

    private string? GetConnectionIdForPath(string filePath)
    {
        lock (_lock)
        {
            var fileDir = Path.GetDirectoryName(filePath);

            foreach (var kvp in _watchers)
            {
                if (!_watchedPaths.TryGetValue(kvp.Key, out var watchedPath))
                    continue;

                // Directory watcher: _watchedPaths stores the directory
                if (fileDir != null && fileDir.Equals(watchedPath, StringComparison.OrdinalIgnoreCase))
                    return kvp.Key;

                // Single-file watcher: _watchedPaths stores the full file path
                if (filePath.Equals(watchedPath, StringComparison.OrdinalIgnoreCase))
                    return kvp.Key;
            }

            return null;
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
