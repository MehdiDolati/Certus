using Certus.Domain.Platform.Interfaces;
using Certus.Infrastructure.Platform;
using FluentAssertions;

namespace Certus.Infrastructure.Tests;

public class FileImportServiceTests : IDisposable
{
    private readonly FileImportService _sut;
    private readonly string _testDirectory;

    public FileImportServiceTests()
    {
        _sut = new FileImportService();
        _testDirectory = Path.Combine(Path.GetTempPath(), $"certus_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDirectory);
    }

    // AC-015: Given a file that doesn't exist, when reading, then null is returned
    [Fact]
    public async Task ReadFileAsync_Should_Return_Null_When_File_Not_Found()
    {
        var result = await _sut.ReadFileAsync(Path.Combine(_testDirectory, "nonexistent.json"));
        result.Should().BeNull();
    }

    // AC-014: Given a locked file, when reading, then the service retries up to 3 times with backoff
    [Fact]
    public async Task ReadFileAsync_Should_Retry_When_File_Locked()
    {
        var filePath = Path.Combine(_testDirectory, "locked.json");
        File.WriteAllText(filePath, "{\"test\": \"data\"}");

        // Lock the file
        using var lockStream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

        // Read should retry and eventually fail (return null) since file is locked
        var result = await _sut.ReadFileAsync(filePath);

        // File is still locked, so result should be null after retries
        result.Should().BeNull();

        lockStream.Close();
    }

    // AC-016: Given a FileSystemWatcher watching a file, when the file changes, then a FileChanged event is raised
    [Fact]
    public void FileSystemWatcher_Should_Raise_Event_On_Change()
    {
        var connectionId = Guid.NewGuid();
        var filePath = Path.Combine(_testDirectory, "portfolio.json");
        File.WriteAllText(filePath, "{\"initial\": true}");

        var eventRaised = false;
        FileImportEventArgs? receivedArgs = null;

        _sut.FileChanged += (sender, args) =>
        {
            eventRaised = true;
            receivedArgs = args;
        };

        _sut.StartWatching(connectionId, filePath);

        // Wait for watcher to be ready
        Thread.Sleep(200);

        // Modify the file
        File.WriteAllText(filePath, "{\"updated\": true}");

        // Wait for event
        Thread.Sleep(500);

        eventRaised.Should().BeTrue();
        receivedArgs.Should().NotBeNull();
        receivedArgs!.ConnectionId.Should().Be(connectionId);
    }

    [Fact]
    public void IsWatching_Should_Return_True_After_Start()
    {
        var connectionId = Guid.NewGuid();
        var filePath = Path.Combine(_testDirectory, "test.json");
        File.WriteAllText(filePath, "{}");

        _sut.StartWatching(connectionId, filePath);

        _sut.IsWatching(connectionId).Should().BeTrue();
    }

    [Fact]
    public void IsWatching_Should_Return_False_After_Stop()
    {
        var connectionId = Guid.NewGuid();
        var filePath = Path.Combine(_testDirectory, "test.json");
        File.WriteAllText(filePath, "{}");

        _sut.StartWatching(connectionId, filePath);
        _sut.StopWatching(connectionId);

        _sut.IsWatching(connectionId).Should().BeFalse();
    }

    public void Dispose()
    {
        _sut.Dispose();
        if (Directory.Exists(_testDirectory))
        {
            try { Directory.Delete(_testDirectory, true); }
            catch { /* cleanup best effort */ }
        }
    }
}
