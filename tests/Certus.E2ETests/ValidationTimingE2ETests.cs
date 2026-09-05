using System.Diagnostics;
using FluentAssertions;

namespace Certus.E2ETests;

// Representative-user timing validation (T071): the new validation pages
// must render within the timing targets a representative user experiences -
// the submission form in under 3 seconds and the detail page in under 3
// seconds from a warm server.
public class ValidationTimingE2ETests : E2ETestBase
{
    [Fact]
    public async Task Validation_Page_Loads_Within_The_Timing_Target()
    {
        if (SkipIfNotAvailable()) return;

        // Warm the server first so the measurement reflects a
        // representative-user page load, not cold-start JIT.
        await Page.GotoAsync($"{BaseUrl}/validation");
        await Page.WaitForSelectorAsync("h1:has-text('Data Validation')", new() { Timeout = 30000 });

        var stopwatch = Stopwatch.StartNew();
        await Page.GotoAsync($"{BaseUrl}/validation");
        await Page.WaitForSelectorAsync("h1:has-text('Data Validation')", new() { Timeout = 30000 });
        stopwatch.Stop();

        stopwatch.Elapsed.TotalSeconds.Should().BeLessThan(3,
            $"the submission form rendered in {stopwatch.Elapsed.TotalSeconds:F2}s; the target is 3s");
    }

    [Fact]
    public async Task Compare_Page_Loads_Within_The_Timing_Target()
    {
        if (SkipIfNotAvailable()) return;

        await Page.GotoAsync($"{BaseUrl}/validation/compare");
        await Page.WaitForSelectorAsync("h1:has-text('Dataset Comparison')", new() { Timeout = 30000 });

        var stopwatch = Stopwatch.StartNew();
        await Page.GotoAsync($"{BaseUrl}/validation/compare");
        await Page.WaitForSelectorAsync("h1:has-text('Dataset Comparison')", new() { Timeout = 30000 });
        stopwatch.Stop();

        stopwatch.Elapsed.TotalSeconds.Should().BeLessThan(3,
            $"the comparison form rendered in {stopwatch.Elapsed.TotalSeconds:F2}s; the target is 3s");
    }
}