using Certus.Application.Platform;
using Certus.Domain.Platform.Enums;
using Certus.Domain.Platform.Interfaces;
using Certus.Domain.Platform.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Certus.Infrastructure.Platform;

public class ConnectionWatcherService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IFileImportService _fileImportService;
    private readonly ILogger<ConnectionWatcherService> _logger;

    public ConnectionWatcherService(
        IServiceScopeFactory scopeFactory,
        IFileImportService fileImportService,
        ILogger<ConnectionWatcherService> logger)
    {
        _scopeFactory = scopeFactory;
        _fileImportService = fileImportService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(2000, stoppingToken);

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var connectionRepo = scope.ServiceProvider.GetRequiredService<IPlatformConnectionRepository>();
            var connections = await connectionRepo.GetAllAsync();

            foreach (var connection in connections)
            {
                if (!connection.Config.UseFileWatcher || string.IsNullOrEmpty(connection.Config.FilePath))
                    continue;

                try
                {
                    _fileImportService.StartWatching(connection.Id, connection.Config.FilePath);
                    RegisterHandler(connection.Id);

                    // Initial import: process existing files so stale data is picked up
                    var platformService = scope.ServiceProvider.GetRequiredService<IPlatformService>();
                    await ImportExistingFiles(platformService, connection);

                    _logger.LogInformation("Restored file watcher for connection {ConnectionId} ({Platform})",
                        connection.Id, connection.PlatformId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to restore watcher for connection {ConnectionId}", connection.Id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restore file watchers on startup");
        }
    }

    private async Task ImportExistingFiles(IPlatformService platformService, Domain.Platform.Aggregates.PlatformConnection connection)
    {
        var filePath = connection.Config.FilePath!;
        string directory;
        string[] patterns;

        if (Directory.Exists(filePath))
        {
            directory = filePath;
            patterns = new[] { "portfolio_status.json", "trades.json" };
        }
        else
        {
            directory = Path.GetDirectoryName(filePath) ?? filePath;
            patterns = new[] { Path.GetFileName(filePath) };
        }

        foreach (var pattern in patterns)
        {
            var fullPath = Path.Combine(directory, pattern);
            if (File.Exists(fullPath))
            {
                try
                {
                    await platformService.ImportFromChangeAsync(connection.Id, fullPath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Initial import failed for {File}", fullPath);
                }
            }
        }
    }

    private void RegisterHandler(Guid connectionId)
    {
        _fileImportService.FileChanged += async (sender, args) =>
        {
            if (args.ConnectionId != connectionId) return;

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<IPlatformService>();
                await handler.ImportFromChangeAsync(connectionId, args.FilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ConnectionWatcherService] Error for {connectionId}: {ex.Message}");
            }
        };
    }
}
