using System.Linq;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using Xunit;

namespace Certus.ComponentTests.Navigation;

// User Story 1 — the primary navigation carries a single "Data Validation"
// entry that reaches /validation, is styled like the other sections, and stays
// highlighted across the whole /validation section. No run-history route/entry
// is added (AC-001, AC-002, AC-018; contracts/navigation.md).
public class NavigationTests : TestContext
{
    public NavigationTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = Bunit.JSRuntimeMode.Loose;
    }

    private static readonly string RunDetailRoute =
        "/validation/runs/" + new string('a', 64);

    [Fact]
    public void Navigation_HasDataValidationEntry_Should_Render_And_Navigate()
    {
        var cut = RenderComponent<Certus.Dashboard.Components.Layout.NavMenu>();

        // AC-001: exactly one "Data Validation" nav link pointing at /validation.
        var validationLinks = cut.FindAll("a.mud-nav-link")
            .Where(a => a.TextContent.Contains("Data Validation"))
            .ToList();
        validationLinks.Should().HaveCount(1);

        // AC-002: the entry's navigation target is /validation (the href is the
        // navigation contract the browser follows on click).
        var dataValidation = validationLinks[0];
        dataValidation.GetAttribute("href").Should().Be("/validation");

        // AC-001: carries a section icon, like the other primary links.
        dataValidation.QuerySelector(".mud-icon-root").Should().NotBeNull();

        // AC-001: same Style/ActiveStyle treatment as the Portfolios link.
        var portfolios = cut.FindAll("a.mud-nav-link")
            .Single(a => a.TextContent.Contains("Portfolios"));
        dataValidation.GetAttribute("style").Should().Be(portfolios.GetAttribute("style"));
    }

    [Theory]
    [InlineData("/validation")]
    [InlineData("/validation/compare")]
    public void Navigation_Entry_Is_Active_Across_The_Section(string route)
    {
        var nav = Services.GetRequiredService<FakeNavigationManager>();
        nav.NavigateTo(route);

        var cut = RenderComponent<Certus.Dashboard.Components.Layout.NavMenu>();

        // AC-002 / FR-002b: prefix match keeps the entry highlighted anywhere in
        // the /validation section (MudBlazor marks the active link with "active").
        var dataValidation = cut.FindAll("a.mud-nav-link")
            .Single(a => a.TextContent.Contains("Data Validation"));
        dataValidation.ClassList.Should().Contain("active");
    }

    [Fact]
    public void Navigation_Entry_Is_Active_On_Run_Detail()
    {
        var nav = Services.GetRequiredService<FakeNavigationManager>();
        nav.NavigateTo(RunDetailRoute);

        var cut = RenderComponent<Certus.Dashboard.Components.Layout.NavMenu>();

        var dataValidation = cut.FindAll("a.mud-nav-link")
            .Single(a => a.TextContent.Contains("Data Validation"));
        dataValidation.ClassList.Should().Contain("active");
    }

    [Fact]
    public void Navigation_RunDetail_No_History_Route_Added()
    {
        var cut = RenderComponent<Certus.Dashboard.Components.Layout.NavMenu>();
        var links = cut.FindAll("a.mud-nav-link");

        // INV-N1: no second, top-level compare nav entry.
        links.Where(a => a.GetAttribute("href") == "/validation/compare")
            .Should().BeEmpty();

        // INV-N2: no run-history nav entry/route exists.
        links.Where(a => (a.GetAttribute("href") ?? string.Empty).Contains("/validation/runs"))
            .Should().BeEmpty();
        links.Where(a => a.TextContent.ToLowerInvariant().Contains("history"))
            .Should().BeEmpty();

        // Only the single Data Validation section entry targets /validation.
        links.Where(a => (a.GetAttribute("href") ?? string.Empty).StartsWith("/validation"))
            .Should().HaveCount(1);
    }
}
