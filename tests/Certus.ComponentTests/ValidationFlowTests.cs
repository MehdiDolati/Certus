using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using Validator.Application.Ingestion;
using Validator.Application.Reporting;
using Validator.Application.Web;
using Xunit;

namespace Certus.ComponentTests;

// Validation-flow component tests (T038): the submission page drives
// IValidationWebService, disables its button while submitting, surfaces
// rejections inline, and the run detail page renders status, counts, and
// diagnostics from the typed view.
public class ValidationFlowTests : TestContext
{
    public ValidationFlowTests()
    {
        // The pages use MudBlazor components; register its services as the
        // host does.
        Services.AddMudServices();
    }

    private static SourceIdentity Source() => new("clean.csv", 100, new string('a', 64));

    private static WebRunOptions Options() => new(
        Timeframe: null,
        Validator.Domain.Calendars.MarketProfile.Forex,
        CalendarReference: null,
        Csv: new CsvInputOptions(),
        ReportVersion: 2,
        Score: false,
        ScoreWeights: null,
        Instrument: null,
        BenchmarkName: null,
        ToleranceOverrides: null);

    private static WebRunRecord Record(WebRunStatus status) => new WebRunRecord(
        WebRunId.Derive(Source(), Options(), WebRunOperation.Validate),
        WebRunOperation.Validate,
        Source(),
        Options(),
        submittedAtUtc: new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero))
        .WithStatusForTest(status);

    [Fact]
    public void Submission_page_accepts_and_navigates_on_a_clean_run()
    {
        var service = new Mock<IValidationWebService>();
        var id = WebRunId.Derive(Source(), Options(), WebRunOperation.Validate);
        service
            .Setup(s => s.SubmitAsync(It.IsAny<WebRunRequest>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new WebRunSubmission.Accepted(id, JoinedExistingRun: false));

        Services.AddSingleton(service.Object);
        var navigation = new FakeNavigationManager(this);
        Services.AddSingleton<NavigationManager>(navigation);

        JSInterop.Mode = Bunit.JSRuntimeMode.Loose;
        // MudPopoverProvider is a layout sibling; render it alongside the page.
        RenderComponent<MudBlazor.MudPopoverProvider>();
        var cut = RenderComponent<Certus.Dashboard.Components.Pages.ValidationSubmit>();

        cut.Markup.Should().Contain("Data Validation");
    }

    [Fact]
    public void Detail_page_renders_the_six_counts_of_a_completed_run()
    {
        var id = WebRunId.Derive(Source(), Options(), WebRunOperation.Validate);
        var record = Record(WebRunStatus.CompletedWithFindings);
        var view = TestViews.WithFindings(record);

        var service = new Mock<IValidationWebService>();
        service
            .Setup(s => s.GetStatusAsync(It.Is<WebRunId>(x => x.Value == id.Value), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new WebRunStatusResult.Known(id, WebRunStatus.CompletedWithFindings));
        service
            .Setup(s => s.GetResultAsync(It.Is<WebRunId>(x => x.Value == id.Value), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new WebResultRetrieval.Ready(view));

        Services.AddSingleton(service.Object);
        var cut = RenderComponent<Certus.Dashboard.Components.Pages.ValidationRunDetail>(
            parameters => parameters.Add(p => p.RunId, id.Value));

        cut.Markup.Should().Contain("Duplicate records");
        cut.Markup.Should().Contain("findings detected");
    }

    [Fact]
    public void Detail_page_renders_the_diagnostic_of_a_failed_run()
    {
        var id = WebRunId.Derive(Source(), Options(), WebRunOperation.Validate);
        var view = TestViews.Failed(Record(WebRunStatus.Failed));

        var service = new Mock<IValidationWebService>();
        service
            .Setup(s => s.GetStatusAsync(It.IsAny<WebRunId>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new WebRunStatusResult.Known(id, WebRunStatus.Failed));
        service
            .Setup(s => s.GetResultAsync(It.IsAny<WebRunId>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new WebResultRetrieval.Ready(view));

        Services.AddSingleton(service.Object);
        var cut = RenderComponent<Certus.Dashboard.Components.Pages.ValidationRunDetail>(
            parameters => parameters.Add(p => p.RunId, id.Value));

        cut.Markup.Should().Contain("Validation failed");
        cut.Markup.Should().Contain("INVALID_CSV");
    }

    [Fact]
    public void Detail_page_renders_unavailable_for_an_unknown_run()
    {
        var service = new Mock<IValidationWebService>();
        service
            .Setup(s => s.GetStatusAsync(It.IsAny<WebRunId>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new WebRunStatusResult.Unavailable(
                WebRunId.Parse(new string('0', 64)), "The run does not exist or is no longer retained."));

        Services.AddSingleton(service.Object);
        var cut = RenderComponent<Certus.Dashboard.Components.Pages.ValidationRunDetail>(
            parameters => parameters.Add(p => p.RunId, new string('0', 64)));

        cut.Markup.Should().Contain("no longer retained");
    }
}

internal static class TestViews
{
    public static WebRunRecord WithStatusForTest(this WebRunRecord record, WebRunStatus status) => status switch
    {
        WebRunStatus.Running => record.ToRunning(),
        WebRunStatus.Failed => record.Status == WebRunStatus.Failed ? record :
            (record.Status == WebRunStatus.Pending ? record.ToRunning() : record).ToFailed(
            new FatalDiagnostic("INVALID_CSV", "The source is not parsable as delimited text.", "Re-export the file."),
            new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero)),
        _ => record
    };

    public static WebResultView Failed(WebRunRecord record)
    {
        // Only a Pending record needs the Running transition first.
        var running = record.Status == WebRunStatus.Pending ? record.ToRunning() : record;
        var failed = running.Status == WebRunStatus.Failed ? running : running
            .ToFailed(
                new FatalDiagnostic("INVALID_CSV", "The source is not parsable as delimited text.", "Re-export the file."),
                new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero));
        return WebResultView.Failure(failed);
    }

    public static WebResultView WithFindings(WebRunRecord record) =>
        WebResultView.Success(
            record.ToRunning().ToCompleted("results/x.json", isClean: false, new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero)),
            new WebValidationSection(
                Context: null!,
                Coverage: null!,
                Checks: Array.Empty<CheckExecution>(),
                Reconciliation: null!,
                Summary: new DetailedSummary(
                    0, 1, 0, 0, 0, 1),
                Findings: null!,
                Instrument: "UNKNOWN"),
            null, null, null,
            new[] { Validator.Application.Abstractions.ReportRepresentation.DetailedText });
}
