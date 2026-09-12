using System.Linq;
using System.Threading;
using Bunit;
using FluentAssertions;
using Moq;
using MudBlazor;
using Validator.Application.Web;
using Xunit;
using DashShared = Certus.Dashboard.Components.Shared;

namespace Certus.ComponentTests.Validation;

// User Story 3 — the run-detail page renders each run state through the standard
// dashboard presentation (status banners, dashboard-styled tables, secondary
// export buttons) with unchanged behaviour (AC-007–AC-013; status map S1–S9).
public class ValidationRunDetailTests : TestContext
{
    private IRenderedComponent<Certus.Dashboard.Components.Pages.ValidationRunDetail> RenderFor(
        WebRunStatus status, WebResultView? view, Mock<IValidationWebService>? existing = null)
    {
        var id = ValidationTestContextExtensions.Id();
        var service = existing ?? new Mock<IValidationWebService>();
        service
            .Setup(s => s.GetStatusAsync(It.IsAny<WebRunId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebRunStatusResult.Known(id, status));
        if (view is not null)
        {
            service
                .Setup(s => s.GetResultAsync(It.IsAny<WebRunId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new WebResultRetrieval.Ready(view));
        }

        this.ConfigureValidation(service);
        return RenderComponent<Certus.Dashboard.Components.Pages.ValidationRunDetail>(
            p => p.Add(x => x.RunId, id.Value));
    }

    private static Severity SeverityOfBanner(
        IRenderedComponent<Certus.Dashboard.Components.Pages.ValidationRunDetail> cut) =>
        cut.FindComponents<MudAlert>().First().Instance.Severity;

    [Theory]
    [InlineData(WebRunStatus.Pending)]
    [InlineData(WebRunStatus.Running)]
    public void ValidationRunDetail_Status_Shows_Info_For_Pending_Or_Running(WebRunStatus status)
    {
        // S2/S3, AC-010: pending/running → Info banner + progress + refresh.
        var cut = RenderFor(status, view: null);

        SeverityOfBanner(cut).Should().Be(Severity.Info);
        cut.FindComponents<MudProgressCircular>().Should().NotBeEmpty();
        cut.FindComponents<MudButton>()
            .Should().Contain(b => b.Markup.Contains("Refresh"));
    }

    [Fact]
    public void ValidationRunDetail_Status_Shows_Success_For_Clean()
    {
        // S4, AC-007: clean → Success banner + quality table inside a card.
        var cut = RenderFor(WebRunStatus.CompletedClean, ValidationTestContextExtensions.CleanView());

        cut.FindComponents<MudAlert>()
            .Should().Contain(a => a.Instance.Severity == Severity.Success);
        cut.FindComponents<DashShared.DashboardCard>().Should().NotBeEmpty();
        cut.Markup.Should().Contain("Missing candles");
    }

    [Fact]
    public void ValidationRunDetail_Status_Shows_Warning_For_Findings_With_All_Counts()
    {
        // S5, AC-008: findings → Warning banner and all six counts remain visible.
        var cut = RenderFor(WebRunStatus.CompletedWithFindings, ValidationTestContextExtensions.FindingsView());

        cut.FindComponents<MudAlert>()
            .Should().Contain(a => a.Instance.Severity == Severity.Warning);
        cut.Markup.Should().Contain("Missing candles");
        cut.Markup.Should().Contain("Duplicate records");
        cut.Markup.Should().Contain("Invalid OHLC");
        cut.Markup.Should().Contain("Closed-market records");
        cut.Markup.Should().Contain("Time gaps");
        cut.Markup.Should().Contain("Malformed rows");
    }

    [Fact]
    public void ValidationRunDetail_Status_Shows_Error_For_Failed_With_Diagnostic()
    {
        // S6, AC-009: failed → Error banner carrying code/reason/guidance.
        var cut = RenderFor(WebRunStatus.Failed, ValidationTestContextExtensions.FailedView());

        cut.FindComponents<MudAlert>()
            .Should().Contain(a => a.Instance.Severity == Severity.Error);
        cut.Markup.Should().Contain("INVALID_CSV");
        cut.Markup.Should().Contain("Re-export the file.");
    }

    [Fact]
    public void ValidationRunDetail_Status_Shows_Warning_Or_EmptyState_For_Unavailable()
    {
        // S7: unavailable run → Warning alert or shared EmptyState (no crash).
        var id = ValidationTestContextExtensions.Id();
        var service = new Mock<IValidationWebService>();
        service
            .Setup(s => s.GetStatusAsync(It.IsAny<WebRunId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebRunStatusResult.Unavailable(id, "The run does not exist or is no longer retained."));
        this.ConfigureValidation(service);

        var cut = RenderComponent<Certus.Dashboard.Components.Pages.ValidationRunDetail>(
            p => p.Add(x => x.RunId, id.Value));

        var hasWarning = cut.FindComponents<MudAlert>()
            .Any(a => a.Instance.Severity == Severity.Warning);
        var hasEmptyState = cut.FindComponents<DashShared.EmptyState>().Any();
        (hasWarning || hasEmptyState).Should().BeTrue();
        cut.Markup.Should().Contain("no longer retained");
    }

    [Fact]
    public void ValidationRunDetail_ExportButtons_Style_Matches_Dashboard()
    {
        // S9, AC-012: one secondary (outlined) button per available export.
        var view = ValidationTestContextExtensions.CleanView();
        var cut = RenderFor(WebRunStatus.CompletedClean, view);

        var exportButtons = cut.FindComponents<MudButton>()
            .Where(b => b.Instance.Variant == Variant.Outlined
                        && b.Markup.Contains("DetailedText"))
            .ToList();
        exportButtons.Should().HaveCount(view.AvailableExports.Count);
        exportButtons.Should().NotBeEmpty();
    }

    [Fact]
    public void ValidationRunDetail_Scores_Render_On_Standard_Typography()
    {
        // S8, AC-013/AC-011: scoring renders dataset average + per-dimension rows
        // in a dashboard-styled table inside a card.
        var cut = RenderFor(WebRunStatus.CompletedClean,
            ValidationTestContextExtensions.CleanView(withScoring: true));

        cut.Markup.Should().Contain("Dataset average");
        cut.Markup.Should().Contain("MissingCandle");
        cut.FindComponents<DashShared.DashboardCard>().Should().NotBeEmpty();
    }
}
