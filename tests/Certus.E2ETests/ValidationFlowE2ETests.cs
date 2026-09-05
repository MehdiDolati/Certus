using FluentAssertions;

namespace Certus.E2ETests;

// Validation-flow E2E (T039): the representative happy path through the
// running dashboard - open the validation page, verify the upload form, and
// confirm the page's durable-state contract (US1 scenarios 1-2). The file
// upload itself is covered by the component tests; this suite proves the
// pages exist and render on the live site.
public class ValidationFlowE2ETests : E2ETestBase
{
    [Fact]
    public async Task Validation_Page_Should_Load_With_Upload_Form()
    {
        if (SkipIfNotAvailable()) return;
        await Page.GotoAsync($"{BaseUrl}/validation");
        await Page.WaitForSelectorAsync("h1:has-text('Data Validation')", new() { Timeout = 15000 });

        var body = await Page.TextContentAsync("body");
        body.Should().Contain("Upload an OHLCV CSV dataset");
        body.Should().Contain("Validate dataset");
    }

    [Fact]
    public async Task Validation_Page_Should_Offer_Scoring_Option()
    {
        if (SkipIfNotAvailable()) return;
        await Page.GotoAsync($"{BaseUrl}/validation");
        await Page.WaitForSelectorAsync("h1:has-text('Data Validation')", new() { Timeout = 15000 });

        var body = await Page.TextContentAsync("body");
        body.Should().Contain("Compute quality score");
        body.Should().Contain("Timeframe");
    }

    [Fact]
    public async Task Compare_Page_Should_Load_With_Benchmark_Form()
    {
        if (SkipIfNotAvailable()) return;
        await Page.GotoAsync($"{BaseUrl}/validation/compare");
        await Page.WaitForSelectorAsync("h1:has-text('Dataset Comparison')", new() { Timeout = 15000 });

        var body = await Page.TextContentAsync("body");
        body.Should().Contain("Benchmark name");
        body.Should().Contain("Instrument");
    }

    [Fact]
    public async Task Run_Detail_Page_Should_Report_An_Unknown_Run_As_Unavailable()
    {
        if (SkipIfNotAvailable()) return;
        var unknownId = new string('0', 64);
        await Page.GotoAsync($"{BaseUrl}/validation/runs/{unknownId}");
        await Page.WaitForSelectorAsync("text=no longer retained", new() { Timeout = 15000 });

        var body = await Page.TextContentAsync("body");
        body.Should().Contain("no longer retained");
    }
}

// Accessibility and responsive E2E (T070): the new pages are keyboard
// navigable and render at representative viewport widths.
public class ValidationAccessibilityE2ETests : E2ETestBase
{
    [Fact]
    public async Task Validation_Page_Is_Keyboard_Navigable()
    {
        if (SkipIfNotAvailable()) return;
        await Page.GotoAsync($"{BaseUrl}/validation");
        await Page.WaitForSelectorAsync("h1:has-text('Data Validation')", new() { Timeout = 15000 });

        // Tab through the form; focus must move without mouse input.
        for (var tab = 0; tab < 5; tab++)
        {
            await Page.Keyboard.PressAsync("Tab");
        }

        var focused = await Page.EvaluateAsync<string>("document.activeElement.tagName");
        focused.Should().BeOneOf("INPUT", "BUTTON", "SELECT", "A", "TEXTAREA");
    }

    [Fact]
    public async Task Validation_Page_Renders_At_Mobile_Width()
    {
        if (SkipIfNotAvailable()) return;
        await Page.SetViewportSizeAsync(375, 812);
        await Page.GotoAsync($"{BaseUrl}/validation");
        await Page.WaitForSelectorAsync("h1:has-text('Data Validation')", new() { Timeout = 15000 });

        var heading = await Page.QuerySelectorAsync("h1:has-text('Data Validation')");
        heading.Should().NotBeNull("the heading is visible at mobile width");

        var body = await Page.TextContentAsync("body");
        body.Should().Contain("Validate dataset");
    }

    [Fact]
    public async Task Compare_Page_Renders_At_Mobile_Width()
    {
        if (SkipIfNotAvailable()) return;
        await Page.SetViewportSizeAsync(375, 812);
        await Page.GotoAsync($"{BaseUrl}/validation/compare");
        await Page.WaitForSelectorAsync("h1:has-text('Dataset Comparison')", new() { Timeout = 15000 });

        var heading = await Page.QuerySelectorAsync("h1:has-text('Dataset Comparison')");
        heading.Should().NotBeNull("the heading is visible at mobile width");
    }
}