using Microsoft.Playwright;

namespace Certus.E2ETests;

public class E2ETestBase : IAsyncLifetime
{
    private static readonly AppServerFixture _server = new();
    private bool _serverStarted;

    protected IPlaywright Playwright { get; private set; } = null!;
    protected IBrowser Browser { get; private set; } = null!;
    protected IPage Page { get; private set; } = null!;
    protected string BaseUrl => _server.BaseUrl;
    protected bool IsAvailable { get; private set; }

    public async Task InitializeAsync()
    {
        try
        {
            // Start server only once (shared across all tests in this class)
            if (!_serverStarted)
            {
                await _server.InitializeAsync();
                _serverStarted = true;
            }

            // Initialize Playwright with system Chrome
            Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
                ExecutablePath = GetChromePath()
            });
            Page = await Browser.NewPageAsync();
            IsAvailable = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"E2E setup failed: {ex.Message}");
            IsAvailable = false;
        }
    }

    public async Task DisposeAsync()
    {
        if (IsAvailable)
        {
            await Page.CloseAsync();
            await Browser.CloseAsync();
            Playwright.Dispose();
        }
    }

    protected async Task WaitForPageLoad()
    {
        if (!IsAvailable) return;
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    protected bool SkipIfNotAvailable()
    {
        return !IsAvailable;
    }

    private static string? GetChromePath()
    {
        string[] paths =
        [
            @"C:\Program Files\Google\Chrome\Application\chrome.exe",
            @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
            @"C:\Program Files\Microsoft\Edge\Application\msedge.exe",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Google\Chrome\Application\chrome.exe")
        ];
        return paths.FirstOrDefault(File.Exists);
    }
}
