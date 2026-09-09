using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using Validator.Application.Web;
using Xunit;
using DashShared = Certus.Dashboard.Components.Shared;

namespace Certus.ComponentTests.Validation;

// User Story 4 — a cross-page sweep proving all three validation pages share the
// dashboard building blocks, contain no legacy/browser-default elements, and use
// the standard loading treatment (AC-014, AC-015, AC-019; G-1/G-2/S1).
//
// NB: bUnit forbids registering services after the first render, so each test
// registers its services exactly once (before any RenderComponent call) and then
// renders whatever pages it needs against that single TestContext.
public class ValidationPagesTests : TestContext
{
    // Register MudBlazor + a validation service mock once, then render the shared
    // MudPopoverProvider. Submit/Compare pages issue no service calls on first
    // render; the mock is pre-seeded so the run-detail page renders a clean run.
    private void ConfigureOnce(out WebRunId id)
    {
        id = ValidationTestContextExtensions.Id();
        var service = new Mock<IValidationWebService>();
        service
            .Setup(s => s.GetStatusAsync(It.IsAny<WebRunId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebRunStatusResult.Known(id, WebRunStatus.CompletedClean));
        service
            .Setup(s => s.GetResultAsync(It.IsAny<WebRunId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebResultRetrieval.Ready(ValidationTestContextExtensions.CleanView()));

        Services.AddMudServices();
        Services.AddSingleton(service.Object);
        JSInterop.Mode = Bunit.JSRuntimeMode.Loose;
        RenderComponent<MudBlazor.MudPopoverProvider>();
    }

    [Fact]
    public void ValidationPages_LoadingState_Style_Matches_Dashboard()
    {
        // AC-014 / S1: first render shows a skeleton (no blank white region).
        // Block the status lookup so the initial loading state is observable.
        var gate = new TaskCompletionSource<WebRunStatusResult>();
        var service = new Mock<IValidationWebService>();
        service
            .Setup(s => s.GetStatusAsync(It.IsAny<WebRunId>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<WebRunStatusResult>(gate.Task));

        Services.AddMudServices();
        Services.AddSingleton(service.Object);
        JSInterop.Mode = Bunit.JSRuntimeMode.Loose;
        RenderComponent<MudBlazor.MudPopoverProvider>();

        var cut = RenderComponent<Certus.Dashboard.Components.Pages.ValidationRunDetail>(
            p => p.Add(x => x.RunId, ValidationTestContextExtensions.Id().Value));

        cut.FindComponents<MudSkeleton>().Should().NotBeEmpty();
    }

    [Fact]
    public void ValidationPages_No_Unstyled_Headings_Or_Light_Surfaces()
    {
        // AC-015 / NFR-002 / G-2: no <h1>, no light elevated MudPaper surfaces.
        ConfigureOnce(out var id);

        var pages = new IRenderedFragment[]
        {
            RenderComponent<Certus.Dashboard.Components.Pages.ValidationSubmit>(),
            RenderComponent<Certus.Dashboard.Components.Pages.ValidationCompare>(),
            RenderComponent<Certus.Dashboard.Components.Pages.ValidationRunDetail>(
                p => p.Add(x => x.RunId, id.Value)),
        };

        foreach (var page in pages)
        {
            page.FindAll("h1").Should().BeEmpty("no browser-default headings may remain");

            // No light elevated MudPaper surface (Elevation="2" default look).
            page.FindAll(".mud-elevation-2").Should().BeEmpty();
        }
    }

    [Fact]
    public void ValidationPages_Shares_Dashboard_Building_Blocks()
    {
        // AC-019 / FR-013 / G-1: each page composes the shared building blocks
        // (structural parity), not one-off lookalike markup.
        ConfigureOnce(out var id);

        var submit = RenderComponent<Certus.Dashboard.Components.Pages.ValidationSubmit>();
        submit.FindComponents<DashShared.PageHeader>().Should().ContainSingle();
        submit.FindComponents<DashShared.DashboardCard>().Should().ContainSingle();

        var compare = RenderComponent<Certus.Dashboard.Components.Pages.ValidationCompare>();
        compare.FindComponents<DashShared.PageHeader>().Should().ContainSingle();
        compare.FindComponents<DashShared.DashboardCard>().Should().ContainSingle();

        var runDetail = RenderComponent<Certus.Dashboard.Components.Pages.ValidationRunDetail>(
            p => p.Add(x => x.RunId, id.Value));
        runDetail.FindComponents<DashShared.PageHeader>().Should().ContainSingle();
        runDetail.FindComponents<DashShared.DashboardCard>().Should().NotBeEmpty();
    }
}
