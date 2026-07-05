using System.Diagnostics;

namespace Certus.E2ETests;

/// <summary>
/// Starts the Certus Dashboard app as a real process for Playwright E2E tests.
/// </summary>
public class AppServerFixture : IAsyncLifetime
{
    private Process? _serverProcess;
    private readonly int _port;

    public string BaseUrl => $"http://localhost:{_port}";
    public bool IsRunning => _serverProcess != null && !_serverProcess.HasExited;

    public AppServerFixture()
    {
        // Use a random available port
        _port = GetAvailablePort();
    }

    public async Task InitializeAsync()
    {
        var dashboardPath = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..",
                "src", "Certus.Dashboard", "Certus.Dashboard.csproj"));

        _serverProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project \"{dashboardPath}\" --urls http://localhost:{_port}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        _serverProcess.Start();

        // Wait for server to be ready (up to 30 seconds)
        await WaitForServerReady(TimeSpan.FromSeconds(30));
    }

    public async Task DisposeAsync()
    {
        if (_serverProcess != null && !_serverProcess.HasExited)
        {
            try
            {
                // Kill the entire process tree (dotnet run spawns child processes)
                KillProcessTree(_serverProcess);
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                await _serverProcess.WaitForExitAsync(cts.Token);
            }
            catch
            {
                // Process may already be gone
            }
            _serverProcess.Dispose();
        }
    }

    private static void KillProcessTree(Process process)
    {
        try
        {
            // On Windows, use taskkill to kill the entire process tree
            if (OperatingSystem.IsWindows())
            {
                var taskkill = Process.Start(new ProcessStartInfo
                {
                    FileName = "taskkill",
                    Arguments = $"/F /T /PID {process.Id}",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                taskkill?.WaitForExit(5000);
            }
            else
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch
        {
            process.Kill();
        }
    }

    private async Task WaitForServerReady(TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        using var client = new HttpClient();

        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var response = await client.GetAsync($"{BaseUrl}/");
                if (response.IsSuccessStatusCode)
                    return;
            }
            catch
            {
                // Server not ready yet
            }

            await Task.Delay(500);
        }

        throw new TimeoutException($"Server did not start within {timeout.TotalSeconds} seconds");
    }

    private static int GetAvailablePort()
    {
        var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}
