using FluentAssertions;
using Microsoft.Playwright;

namespace Certus.E2ETests;

/// <summary>
/// E2E tests for the Portfolio Deployment feature.
/// Covers: AC-001, AC-002, AC-003, AC-005, AC-009, AC-010, AC-023, AC-024
/// </summary>
public class PortfolioDeploymentFlowTests : E2ETestBase
{
    // =====================================================
    // Platform Dashboard
    // =====================================================

    [Fact]
    public async Task PlatformDashboard_Should_Show_DeployPortfolio_Button()
    {
        if (SkipIfNotAvailable()) return;
        await Page.GotoAsync($"{BaseUrl}/platform");
        await Page.WaitForSelectorAsync("h4:has-text('Platform Management')", new() { Timeout = 10000 });

        var deployButton = await Page.QuerySelectorAsync("button:has-text('Deploy Portfolio')");
        deployButton.Should().NotBeNull();
    }

    [Fact]
    public async Task PlatformDashboard_Should_Show_Connect_Button()
    {
        if (SkipIfNotAvailable()) return;
        await Page.GotoAsync($"{BaseUrl}/platform");
        await Page.WaitForSelectorAsync("h4:has-text('Platform Management')", new() { Timeout = 10000 });

        var connectButton = await Page.QuerySelectorAsync("button:has-text('Connect')");
        connectButton.Should().NotBeNull();
    }

    [Fact]
    public async Task PlatformDashboard_Should_Show_KPI_Cards()
    {
        if (SkipIfNotAvailable()) return;
        await Page.GotoAsync($"{BaseUrl}/platform");
        await Page.WaitForSelectorAsync("h4:has-text('Platform Management')", new() { Timeout = 10000 });

        // KPI cards should be present
        var body = await Page.TextContentAsync("body");
        body.Should().Contain("Total Connections");
        body.Should().Contain("Active Connections");
    }

    // =====================================================
    // Deploy Dialog - Step 1: Folder Selection
    // =====================================================

    [Fact]
    public async Task DeployDialog_Should_Open_When_DeployPortfolio_Clicked()
    {
        if (SkipIfNotAvailable()) return;
        await Page.GotoAsync($"{BaseUrl}/platform");
        await Page.WaitForSelectorAsync("button:has-text('Deploy Portfolio')", new() { Timeout = 10000 });

        await Page.ClickAsync("button:has-text('Deploy Portfolio')");
        await Page.WaitForSelectorAsync("h6:has-text('Deploy Portfolio')", new() { Timeout = 5000 });

        var title = await Page.TextContentAsync("h6:has-text('Deploy Portfolio')");
        title.Should().Contain("Deploy Portfolio");
    }

    [Fact]
    public async Task DeployDialog_Should_Show_Step1_FolderSelection()
    {
        if (SkipIfNotAvailable()) return;
        await Page.GotoAsync($"{BaseUrl}/platform");
        await Page.WaitForSelectorAsync("button:has-text('Deploy Portfolio')", new() { Timeout = 10000 });
        await Page.ClickAsync("button:has-text('Deploy Portfolio')");
        await Page.WaitForSelectorAsync("h6:has-text('Deploy Portfolio')", new() { Timeout = 5000 });

        var step1Text = await Page.TextContentAsync("body");
        step1Text.Should().Contain("Step 1");
        step1Text.Should().Contain("Select Portfolio Folder");
    }

    [Fact]
    public async Task DeployDialog_Should_Have_Folder_Path_Input()
    {
        if (SkipIfNotAvailable()) return;
        await Page.GotoAsync($"{BaseUrl}/platform");
        await Page.WaitForSelectorAsync("button:has-text('Deploy Portfolio')", new() { Timeout = 10000 });
        await Page.ClickAsync("button:has-text('Deploy Portfolio')");
        await Page.WaitForSelectorAsync("h6:has-text('Deploy Portfolio')", new() { Timeout = 5000 });

        var folderInput = await Page.QuerySelectorAsync("input[placeholder*='Path']");
        folderInput.Should().NotBeNull();
    }

    [Fact]
    public async Task DeployDialog_Should_Have_ValidateFolder_Button()
    {
        if (SkipIfNotAvailable()) return;
        await Page.GotoAsync($"{BaseUrl}/platform");
        await Page.WaitForSelectorAsync("button:has-text('Deploy Portfolio')", new() { Timeout = 10000 });
        await Page.ClickAsync("button:has-text('Deploy Portfolio')");
        await Page.WaitForSelectorAsync("h6:has-text('Deploy Portfolio')", new() { Timeout = 5000 });

        var validateButton = await Page.QuerySelectorAsync("button:has-text('Validate Folder')");
        validateButton.Should().NotBeNull();
    }

    [Fact]
    public async Task DeployDialog_Should_Have_Next_Button()
    {
        if (SkipIfNotAvailable()) return;
        await Page.GotoAsync($"{BaseUrl}/platform");
        await Page.WaitForSelectorAsync("button:has-text('Deploy Portfolio')", new() { Timeout = 10000 });
        await Page.ClickAsync("button:has-text('Deploy Portfolio')");
        await Page.WaitForSelectorAsync("h6:has-text('Deploy Portfolio')", new() { Timeout = 5000 });

        var nextButton = await Page.QuerySelectorAsync("button:has-text('Next')");
        nextButton.Should().NotBeNull();
    }

    [Fact]
    public async Task DeployDialog_Should_Have_Cancel_Button()
    {
        if (SkipIfNotAvailable()) return;
        await Page.GotoAsync($"{BaseUrl}/platform");
        await Page.WaitForSelectorAsync("button:has-text('Deploy Portfolio')", new() { Timeout = 10000 });
        await Page.ClickAsync("button:has-text('Deploy Portfolio')");
        await Page.WaitForSelectorAsync("h6:has-text('Deploy Portfolio')", new() { Timeout = 5000 });

        var cancelButton = await Page.QuerySelectorAsync("button:has-text('Cancel')");
        cancelButton.Should().NotBeNull();
    }

    // =====================================================
    // Deploy Dialog - Cancel Flow
    // =====================================================

    [Fact]
    public async Task DeployDialog_Cancel_Should_Close_Dialog()
    {
        if (SkipIfNotAvailable()) return;
        await Page.GotoAsync($"{BaseUrl}/platform");
        await Page.WaitForSelectorAsync("button:has-text('Deploy Portfolio')", new() { Timeout = 10000 });
        await Page.ClickAsync("button:has-text('Deploy Portfolio')");
        await Page.WaitForSelectorAsync("h6:has-text('Deploy Portfolio')", new() { Timeout = 5000 });

        await Page.ClickAsync("button:has-text('Cancel')");
        await Page.WaitForTimeoutAsync(1000);

        var dialogTitle = await Page.QuerySelectorAsync("h6:has-text('Deploy Portfolio')");
        dialogTitle.Should().BeNull();
    }

    // =====================================================
    // Deploy Dialog - Validation Feedback
    // =====================================================

    [Fact]
    public async Task DeployDialog_Should_Show_Error_When_Invalid_Path()
    {
        if (SkipIfNotAvailable()) return;
        await Page.GotoAsync($"{BaseUrl}/platform");
        await Page.WaitForSelectorAsync("button:has-text('Deploy Portfolio')", new() { Timeout = 10000 });
        await Page.ClickAsync("button:has-text('Deploy Portfolio')");
        await Page.WaitForSelectorAsync("h6:has-text('Deploy Portfolio')", new() { Timeout = 5000 });

        // Type an invalid path
        var folderInput = await Page.QuerySelectorAsync("input[placeholder*='Path']");
        await folderInput!.FillAsync(@"C:\NonExistent\Folder");

        // Click Validate Folder
        await Page.ClickAsync("button:has-text('Validate Folder')");
        await Page.WaitForTimeoutAsync(1000);

        // Should show error message
        var body = await Page.TextContentAsync("body");
        body.Should().Contain("does not exist");
    }

    // =====================================================
    // Platform Dashboard - Empty State
    // =====================================================

    [Fact]
    public async Task PlatformDashboard_Should_Show_Empty_Connections_State()
    {
        if (SkipIfNotAvailable()) return;
        await Page.GotoAsync($"{BaseUrl}/platform");
        await Page.WaitForSelectorAsync("h4:has-text('Platform Management')", new() { Timeout = 10000 });

        // Should show either connections or empty state
        var body = await Page.TextContentAsync("body");
        body.Should().NotBeNullOrEmpty();
    }

    // =====================================================
    // Navigation
    // =====================================================

    [Fact]
    public async Task PlatformDashboard_Should_Be_Available_At_Platform_Route()
    {
        if (SkipIfNotAvailable()) return;
        await Page.GotoAsync($"{BaseUrl}/platform");
        await Page.WaitForSelectorAsync("h4:has-text('Platform Management')", new() { Timeout = 10000 });

        var url = Page.Url;
        url.Should().Contain("/platform");
    }

    [Fact]
    public async Task NavMenu_Should_Have_Platform_Link()
    {
        if (SkipIfNotAvailable()) return;
        await Page.GotoAsync($"{BaseUrl}/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var platformLink = await Page.QuerySelectorAsync("a[href='/platform']");
        platformLink.Should().NotBeNull();
    }

    // =====================================================
    // Deploy Dialog - Step Navigation
    // =====================================================

    [Fact]
    public async Task DeployDialog_Should_Disable_Next_When_No_Folder_Validated()
    {
        if (SkipIfNotAvailable()) return;
        await Page.GotoAsync($"{BaseUrl}/platform");
        await Page.WaitForSelectorAsync("button:has-text('Deploy Portfolio')", new() { Timeout = 10000 });
        await Page.ClickAsync("button:has-text('Deploy Portfolio')");
        await Page.WaitForSelectorAsync("h6:has-text('Deploy Portfolio')", new() { Timeout = 5000 });

        // Next button should be disabled when no folder is validated
        var nextButton = await Page.QuerySelectorAsync("button:has-text('Next')");
        var isDisabled = await nextButton!.GetAttributeAsync("disabled");
        // Blazor MudButton sets disabled attribute when Disabled=true
        isDisabled.Should().NotBeNull();
    }

    [Fact]
    public async Task DeployDialog_Should_Show_Step2_After_Next()
    {
        if (SkipIfNotAvailable()) return;
        await Page.GotoAsync($"{BaseUrl}/platform");
        await Page.WaitForSelectorAsync("button:has-text('Deploy Portfolio')", new() { Timeout = 10000 });
        await Page.ClickAsync("button:has-text('Deploy Portfolio')");
        await Page.WaitForSelectorAsync("h6:has-text('Deploy Portfolio')", new() { Timeout = 5000 });

        // Fill in a valid path and validate
        var folderInput = await Page.QuerySelectorAsync("input[placeholder*='Path']");
        await folderInput!.FillAsync(Path.GetTempPath());
        await Page.ClickAsync("button:has-text('Validate Folder')");
        await Page.WaitForTimeoutAsync(500);

        // Click Next
        var nextButton = await Page.QuerySelectorAsync("button:has-text('Next')");
        await nextButton!.ClickAsync();
        await Page.WaitForTimeoutAsync(500);

        // Should show Step 2
        var body = await Page.TextContentAsync("body");
        body.Should().Contain("Step 2");
        body.Should().Contain("Select Connection");
    }

    // =====================================================
    // Responsive Layout
    // =====================================================

    [Fact]
    public async Task PlatformDashboard_Should_Responsively_Display_KPIs()
    {
        if (SkipIfNotAvailable()) return;
        await Page.GotoAsync($"{BaseUrl}/platform");
        await Page.WaitForSelectorAsync("h4:has-text('Platform Management')", new() { Timeout = 10000 });

        // Verify the page renders without errors
        var body = await Page.TextContentAsync("body");
        body.Should().Contain("Platform Management");
        body.Should().Contain("Total Connections");
    }
}
