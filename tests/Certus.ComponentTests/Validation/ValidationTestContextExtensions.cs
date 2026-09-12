using System;
using System.Collections.Generic;
using System.Linq;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using Validator.Application.Abstractions;
using Validator.Application.Ingestion;
using Validator.Application.Reporting;
using Validator.Application.Scoring;
using Validator.Application.Web;
using Validator.Domain.Calendars;
using Validator.Domain.Findings;
using Validator.Domain.Scoring;

namespace Certus.ComponentTests.Validation;

// Reusable bUnit test infrastructure for the Validation-UI-Compliance feature
// (T003). Mirrors the existing pattern in ValidationFlowTests.cs: registers
// MudBlazor services, sets a loose JSInterop, renders a MudPopoverProvider so
// MudSelect popovers resolve, and exposes builders for a mocked
// IValidationWebService plus the read-only run views the pages project.
internal static class ValidationTestContextExtensions
{
    // Wire up the MudBlazor host services + a loose JSInterop and render the
    // popover provider (a layout sibling) so pages that use MudSelect/MudPopover
    // render without error. Registers the supplied service (and optional
    // navigation manager) into DI, then returns the mock for further setup.
    public static Mock<IValidationWebService> ConfigureValidation(
        this TestContext ctx,
        Mock<IValidationWebService>? service = null,
        NavigationManager? navigation = null)
    {
        service ??= new Mock<IValidationWebService>();

        ctx.Services.AddMudServices();
        ctx.Services.AddSingleton(service.Object);
        if (navigation is not null)
        {
            ctx.Services.AddSingleton(navigation);
        }

        ctx.JSInterop.Mode = Bunit.JSRuntimeMode.Loose;
        ctx.RenderComponent<MudBlazor.MudPopoverProvider>();

        return service;
    }

    // ---- Read-only view/record builders (behaviour is never modified) --------

    public static SourceIdentity Source() => new("clean.csv", 100, new string('a', 64));

    public static WebRunOptions Options(bool score = false) => new(
        Timeframe: null,
        MarketProfile.Forex,
        CalendarReference: null,
        Csv: new CsvInputOptions(),
        ReportVersion: 2,
        Score: score,
        ScoreWeights: null,
        Instrument: null,
        BenchmarkName: null,
        ToleranceOverrides: null);

    public static WebRunId Id() => WebRunId.Derive(Source(), Options(), WebRunOperation.Validate);

    public static WebRunRecord Record() => new(
        Id(),
        WebRunOperation.Validate,
        Source(),
        Options(),
        submittedAtUtc: new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));

    private static readonly DateTimeOffset CompletedAt = new(2026, 1, 2, 0, 0, 0, TimeSpan.Zero);

    // A completed-clean run view (all quality counts zero), optionally with a
    // scoring section attached.
    public static WebResultView CleanView(bool withScoring = false) =>
        WebResultView.Success(
            Record().ToRunning().ToCompleted("results/x.json", isClean: true, CompletedAt),
            ValidationSection(new DetailedSummary(0, 0, 0, 0, 0, 0)),
            withScoring ? Scoring() : null,
            null,
            null,
            new[] { ReportRepresentation.DetailedText });

    // A completed-with-findings run view with visible non-zero counts.
    public static WebResultView FindingsView() =>
        WebResultView.Success(
            Record().ToRunning().ToCompleted("results/x.json", isClean: false, CompletedAt),
            ValidationSection(new DetailedSummary(2, 1, 3, 0, 4, 1)),
            null,
            null,
            null,
            new[] { ReportRepresentation.DetailedText });

    // A failed run view carrying a fatal diagnostic (code/reason/guidance).
    public static WebResultView FailedView()
    {
        var failed = Record().ToRunning().ToFailed(
            new FatalDiagnostic("INVALID_CSV", "The source is not parsable as delimited text.", "Re-export the file."),
            CompletedAt);
        return WebResultView.Failure(failed);
    }

    private static WebValidationSection ValidationSection(DetailedSummary summary) => new(
        Context: null!,
        Coverage: null!,
        Checks: Array.Empty<CheckExecution>(),
        Reconciliation: null!,
        Summary: summary,
        Findings: null!,
        Instrument: "UNKNOWN");

    // The six established quality-metric categories the scorer covers.
    private static readonly FindingCategory[] MetricCategories =
    {
        FindingCategory.MissingCandle,
        FindingCategory.DuplicateRecord,
        FindingCategory.InvalidOhlc,
        FindingCategory.ClosedMarketRecord,
        FindingCategory.TimeGap,
        FindingCategory.MalformedRow,
    };

    // Build a minimal-but-valid scoring section: all six dimensions scored and a
    // dataset average of 95 (projected exactly as the pipeline would compute it).
    // DatasetScore.Available requires covered + excluded categories to total six.
    public static WebScoringSection Scoring()
    {
        var average = new ScoreValue(new ExactRatio(95, 1));
        var metrics = MetricCategories
            .Select(category => MetricScore.Scored(
                category, 0L, 100L, MetricPopulationKind.ExpectedCandles, average))
            .ToList();
        var dataset = DatasetScore.Available(
            average,
            MetricCategories.ToList(),
            new List<ExcludedMetric>());
        var report = new DatasetScoreReport(metrics, ScoreWeightResolver.Default(), dataset);
        return new WebScoringSection(report);
    }
}
