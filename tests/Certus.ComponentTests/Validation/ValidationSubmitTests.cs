using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using Validator.Application.Reporting;
using Validator.Application.Web;
using Xunit;
using DashShared = Certus.Dashboard.Components.Shared;

namespace Certus.ComponentTests.Validation;

// User Story 2 — the submission page is rebuilt on the shared building blocks
// (PageHeader + DashboardCard), shows the standard busy treatment with duplicate
// prevention, renders rejection/upload errors as inline error alerts, offers the
// in-section compare action, and navigates without a full reload on success
// (AC-003, AC-004, AC-005, AC-006, AC-017, AC-020).
public class ValidationSubmitTests : TestContext
{
    private static InputFileContent Csv() =>
        InputFileContent.CreateFromText("timestamp,open,high,low,close,volume\n1,1,1,1,1,1", "data.csv");

    [Fact]
    public void ValidationSubmit_HeaderAndCard_Style_Matches_Dashboard()
    {
        // Block the submission so the busy state is observable mid-flight.
        var gate = new TaskCompletionSource<WebRunSubmission>();
        var service = new Mock<IValidationWebService>();
        service
            .Setup(s => s.SubmitAsync(It.IsAny<WebRunRequest>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<WebRunSubmission>(gate.Task));
        this.ConfigureValidation(service);

        var cut = RenderComponent<Certus.Dashboard.Components.Pages.ValidationSubmit>();

        // AC-003: the page is composed from the shared building blocks.
        cut.FindComponents<DashShared.PageHeader>().Should().ContainSingle();
        cut.FindComponents<DashShared.DashboardCard>().Should().ContainSingle();

        // AC-005: select a file and submit; the button shows the busy treatment.
        cut.FindComponent<InputFile>().UploadFiles(Csv());
        var submit = cut.FindAll("button").Single(b => b.TextContent.Contains("Validate"));
        submit.Click();

        var busy = cut.FindAll("button").Single(b => b.TextContent.Contains("Validating"));
        busy.HasAttribute("disabled").Should().BeTrue("a submission in flight prevents duplicate submits");
        cut.FindComponents<MudProgressCircular>().Should().NotBeEmpty();

        gate.SetResult(new WebRunSubmission.Rejected(
            new FatalDiagnostic("INVALID_CSV", "reason", "guidance")));
    }

    [Fact]
    public void ValidationSubmit_Rejection_Shows_Alert_Style()
    {
        var service = new Mock<IValidationWebService>();
        service
            .Setup(s => s.SubmitAsync(It.IsAny<WebRunRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebRunSubmission.Rejected(
                new FatalDiagnostic("INVALID_CSV", "The source is not parsable.", "Re-export the file.")));
        this.ConfigureValidation(service);

        var cut = RenderComponent<Certus.Dashboard.Components.Pages.ValidationSubmit>();
        cut.FindComponent<InputFile>().UploadFiles(Csv());
        cut.FindAll("button").Single(b => b.TextContent.Contains("Validate")).Click();

        // AC-006: the rejection renders as an error alert (code/reason/guidance),
        // not a plain-text strip.
        var alerts = cut.FindComponents<MudAlert>();
        alerts.Should().Contain(a => a.Instance.Severity == Severity.Error);
        cut.Markup.Should().Contain("INVALID_CSV");
        cut.Markup.Should().Contain("Re-export the file.");
    }

    [Fact]
    public void ValidationSubmit_UploadError_Shows_Alert_Style()
    {
        var service = new Mock<IValidationWebService>();
        service
            .Setup(s => s.SubmitAsync(It.IsAny<WebRunRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new System.IO.IOException("stream broke"));
        this.ConfigureValidation(service);

        var cut = RenderComponent<Certus.Dashboard.Components.Pages.ValidationSubmit>();
        cut.FindComponent<InputFile>().UploadFiles(Csv());
        cut.FindAll("button").Single(b => b.TextContent.Contains("Validate")).Click();

        // AC-006 (U4): an upload/read failure renders as an inline error alert.
        cut.FindComponents<MudAlert>().Should().Contain(a => a.Instance.Severity == Severity.Error);
    }

    [Fact]
    public void ValidationSubmit_CompareOption_Visible_Inside_Section()
    {
        this.ConfigureValidation();
        var cut = RenderComponent<Certus.Dashboard.Components.Pages.ValidationSubmit>();

        // AC-017 / INV-N1: the compare submission page is reachable via an
        // in-section secondary action (an anchor to /validation/compare), not a
        // separate top-level nav entry.
        cut.FindAll("a")
            .Where(a => a.GetAttribute("href") == "/validation/compare")
            .Should().ContainSingle();
    }

    [Fact]
    public void ValidationSubmit_Success_Navigates_Without_Reload()
    {
        var id = ValidationTestContextExtensions.Id();
        var service = new Mock<IValidationWebService>();
        service
            .Setup(s => s.SubmitAsync(It.IsAny<WebRunRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebRunSubmission.Accepted(id, JoinedExistingRun: false));
        this.ConfigureValidation(service);

        var nav = Services.GetRequiredService<FakeNavigationManager>();
        var cut = RenderComponent<Certus.Dashboard.Components.Pages.ValidationSubmit>();
        cut.FindComponent<InputFile>().UploadFiles(Csv());
        cut.FindAll("button").Single(b => b.TextContent.Contains("Validate")).Click();

        // AC-020 / INV-R4: an accepted submission navigates to the run detail
        // without a full page reload (forceLoad == false).
        nav.Uri.Should().Be(nav.BaseUri + $"validation/runs/{id.Value}");
        nav.History.Should().NotBeEmpty();
        nav.History.Last().Options.ForceLoad.Should().BeFalse();
    }
}
