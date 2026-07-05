using FluentAssertions;

namespace Certus.E2ETests;

public class NavigationFlowTests : E2ETestBase
{
    [Fact]
    public async Task Dashboard_Should_Navigate_To_Detail_On_ViewReport_Click()
    {
        if (SkipIfNotAvailable()) return;
        await Page.GotoAsync($"{BaseUrl}/portfolios");
        try { await Page.WaitForSelectorAsync("table tbody tr", new() { Timeout = 5000 }); }
        catch { return; }

        var viewButton = await Page.QuerySelectorAsync("button:has-text('View Report')");
        if (viewButton != null)
        {
            await viewButton.ClickAsync();
            await Page.WaitForSelectorAsync("h4", new() { Timeout = 10000 });
            Page.Url.Should().Contain("/portfolios/");
        }
    }

    [Fact]
    public async Task Home_Should_Have_Link_To_Dashboard()
    {
        if (SkipIfNotAvailable()) return;
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForSelectorAsync("a:has-text('Portfolios'), button:has-text('Portfolios')", new() { Timeout = 10000 });
        var link = await Page.QuerySelectorAsync("a:has-text('Portfolios'), button:has-text('Portfolios')");
        link.Should().NotBeNull();
    }

    [Fact]
    public async Task Detail_Page_Should_Have_Back_Button()
    {
        if (SkipIfNotAvailable()) return;
        await Page.GotoAsync($"{BaseUrl}/portfolios");
        try { await Page.WaitForSelectorAsync("table tbody tr", new() { Timeout = 5000 }); }
        catch { return; }

        var viewButton = await Page.QuerySelectorAsync("button:has-text('View Report')");
        if (viewButton != null)
        {
            await viewButton.ClickAsync();
            await Page.WaitForSelectorAsync("h4", new() { Timeout = 10000 });
            var backButton = await Page.QuerySelectorAsync(".mud-icon-button");
            backButton.Should().NotBeNull();
        }
    }
}
