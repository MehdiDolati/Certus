using FluentAssertions;

namespace Certus.E2ETests;

public class PortfolioCrudFlowTests : E2ETestBase
{
    [Fact]
    public async Task CreatePortfolio_Should_Open_Dialog()
    {
        if (SkipIfNotAvailable()) return;
        await Page.GotoAsync($"{BaseUrl}/portfolios");
        await Page.WaitForSelectorAsync("button:has-text('New Portfolio')", new() { Timeout = 10000 });

        await Page.ClickAsync("button:has-text('New Portfolio')");
        await Page.WaitForSelectorAsync("h6:has-text('Create Portfolio')", new() { Timeout = 5000 });

        var title = await Page.TextContentAsync("h6");
        title.Should().Contain("Create Portfolio");
    }

    [Fact]
    public async Task CreatePortfolio_Should_Have_Form_Fields()
    {
        if (SkipIfNotAvailable()) return;
        await Page.GotoAsync($"{BaseUrl}/portfolios");
        await Page.WaitForSelectorAsync("button:has-text('New Portfolio')", new() { Timeout = 10000 });

        await Page.ClickAsync("button:has-text('New Portfolio')");
        await Page.WaitForSelectorAsync("h6:has-text('Create Portfolio')", new() { Timeout = 5000 });

        var inputs = await Page.QuerySelectorAllAsync("input");
        inputs.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreatePortfolio_Cancel_Should_Close_Dialog()
    {
        if (SkipIfNotAvailable()) return;
        await Page.GotoAsync($"{BaseUrl}/portfolios");
        await Page.WaitForSelectorAsync("button:has-text('New Portfolio')", new() { Timeout = 10000 });

        await Page.ClickAsync("button:has-text('New Portfolio')");
        await Page.WaitForSelectorAsync("h6:has-text('Create Portfolio')", new() { Timeout = 5000 });

        await Page.ClickAsync("button:has-text('Cancel')");
        await Page.WaitForTimeoutAsync(1000);

        var dialogTitle = await Page.QuerySelectorAsync("h6:has-text('Create Portfolio')");
        dialogTitle.Should().BeNull();
    }
}
