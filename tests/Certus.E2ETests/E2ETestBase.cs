using Microsoft.Playwright;

namespace Certus.E2ETests;

public class E2ETestBase : IAsyncLifetime
{
    protected IPlaywright Playwright { get; private set; } = null!;
    protected IBrowser Browser { get; private set; } = null!;
    protected IPage Page { get; private set; } = null!;
    protected string BaseUrl { get; private set; } = "http://localhost:5080";
    protected bool IsAvailable { get; private set; }

    public async Task InitializeAsync()
    {
        try
        {
            Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
                ExecutablePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe"
            });
            Page = await Browser.NewPageAsync();
            IsAvailable = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Playwright init failed: {ex.Message}");
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
}
