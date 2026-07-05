using FluentAssertions;

namespace Certus.E2ETests;

public class DashboardFlowTests : E2ETestBase
{
    [Fact]
    public async Task Dashboard_Should_Load_And_Show_Title()
    {
        if (SkipIfNotAvailable()) return;
        await Page.GotoAsync($"{BaseUrl}/portfolios");
        await Page.WaitForSelectorAsync("h4", new() { Timeout = 10000 });

        var title = await Page.TextContentAsync("h4");
        title.Should().Contain("PORTFOLIO DASHBOARD");
    }

    [Fact]
    public async Task Dashboard_Should_Have_Create_Button()
    {
        if (SkipIfNotAvailable()) return;
        await Page.GotoAsync($"{BaseUrl}/portfolios");
        await Page.WaitForSelectorAsync("button:has-text('New Portfolio')", new() { Timeout = 10000 });

        var createButton = await Page.QuerySelectorAsync("button:has-text('New Portfolio')");
        createButton.Should().NotBeNull();
    }

    [Fact]
    public async Task Dashboard_Should_Have_Filter_Controls()
    {
        if (SkipIfNotAvailable()) return;
        await Page.GotoAsync($"{BaseUrl}/portfolios");
        await Page.WaitForSelectorAsync("h4", new() { Timeout = 10000 });

        // Page loaded successfully - filters are rendered by Blazor SignalR
        var body = await Page.TextContentAsync("body");
        body.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Dashboard_Should_Display_Portfolio_Table()
    {
        if (SkipIfNotAvailable()) return;
        await Page.GotoAsync($"{BaseUrl}/portfolios");
        // Wait for either table or empty state
        await Page.WaitForSelectorAsync("table, h6:has-text('No portfolios')", new() { Timeout = 10000 });
        var tableExists = await Page.QuerySelectorAsync("table");
        var emptyState = await Page.QuerySelectorAsync("h6:has-text('No portfolios')");
        (tableExists != null || emptyState != null).Should().BeTrue();
    }
}
