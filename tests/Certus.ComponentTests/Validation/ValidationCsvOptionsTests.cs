using System;
using System.Linq;
using System.Threading;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Forms;
using Moq;
using Validator.Application.Web;
using Xunit;

namespace Certus.ComponentTests.Validation;

// Regression tests for the MT4 headerless-CSV web validation bug: the
// submission and compare pages previously hardcoded HasHeader = true, so a
// standard MetaTrader export (no header row) failed with INVALID_STRUCTURE
// ("Required header 'date' was not found") while the CLI — which defaults to
// headerless — validated the same file. Both pages now default to headerless
// with delimiter auto-detection (CLI parity) and expose the header-row option.
public class ValidationCsvOptionsTests : TestContext
{
    private static InputFileContent HeaderlessMt4Csv() =>
        InputFileContent.CreateFromText(
            "2018.01.22,15:00,0.99862,0.99943,0.99829,0.99891,2325",
            "AUDCAD60.csv");

    // A service mock that records the options of the most recent submission
    // and accepts it, so assertions can inspect exactly what the page sent.
    private Mock<IValidationWebService> CapturingService(out Func<WebRunOptions?> capture)
    {
        WebRunOptions? captured = null;
        var service = new Mock<IValidationWebService>();
        service
            .Setup(s => s.SubmitAsync(It.IsAny<WebRunRequest>(), It.IsAny<CancellationToken>()))
            .Callback<WebRunRequest, CancellationToken>((request, _) => captured = request.Options)
            .ReturnsAsync(new WebRunSubmission.Accepted(
                ValidationTestContextExtensions.Id(), JoinedExistingRun: false));
        capture = () => captured;
        return service;
    }

    [Fact]
    public void ValidationSubmit_Defaults_To_Headerless_Auto_Delimiter_Like_Cli()
    {
        var service = CapturingService(out var capture);
        this.ConfigureValidation(service);

        var cut = RenderComponent<Certus.Dashboard.Components.Pages.ValidationSubmit>();
        cut.FindComponent<InputFile>().UploadFiles(HeaderlessMt4Csv());
        cut.FindAll("button").Single(b => b.TextContent.Contains("Validate")).Click();

        // The CLI validates a headerless MT4 export by default; the page must
        // send the same interpretation.
        var options = capture();
        options.Should().NotBeNull("the page must submit to the validation service");
        options!.Csv.Should().NotBeNull();
        options.Csv!.HasHeader.Should().BeFalse(
            "MetaTrader exports have no header row and the CLI defaults to headerless");
        options.Csv.Delimiter.Should().BeNull("auto-detection matches the CLI default");
    }

    [Fact]
    public void ValidationSubmit_Header_Option_Reaches_The_Validator()
    {
        var service = CapturingService(out var capture);
        this.ConfigureValidation(service);

        var cut = RenderComponent<Certus.Dashboard.Components.Pages.ValidationSubmit>();
        cut.FindComponent<InputFile>().UploadFiles(HeaderlessMt4Csv());

        // Opt in for datasets that do carry a header row by toggling the
        // rendered checkbox control.
        var headerToggle = cut.FindComponents<MudBlazor.MudCheckBox<bool>>()
            .Single(c => c.Instance.Label?.Contains("header row") == true);
        headerToggle.Find("input").Change(true);

        cut.FindAll("button").Single(b => b.TextContent.Contains("Validate")).Click();

        var options = capture();
        options.Should().NotBeNull("the page must submit to the validation service");
        options!.Csv.Should().NotBeNull();
        options.Csv!.HasHeader.Should().BeTrue("the header option must flow to the validator");
    }

    [Fact]
    public void ValidationCompare_Defaults_To_Headerless_Auto_Delimiter_Like_Cli()
    {
        var service = CapturingService(out var capture);
        this.ConfigureValidation(service);

        var cut = RenderComponent<Certus.Dashboard.Components.Pages.ValidationCompare>();
        cut.FindComponent<InputFile>().UploadFiles(HeaderlessMt4Csv());

        // Fill the two required MudTextFields: benchmark name, instrument.
        var textInputs = cut.FindAll("input.mud-input-root").ToList();
        textInputs.Should().HaveCountGreaterThanOrEqualTo(2);
        textInputs[0].Change("audcad-benchmark");
        textInputs[1].Change("AUDCAD");

        cut.FindAll("button").Single(b => b.TextContent.Contains("Compare")).Click();

        var options = capture();
        options.Should().NotBeNull("the page must submit to the validation service");
        options!.Instrument.Should().Be("AUDCAD", "the instrument field binding must fire");
        options.Csv.Should().NotBeNull();
        options.Csv!.HasHeader.Should().BeFalse(
            "MetaTrader exports have no header row and the CLI defaults to headerless");
        options.Csv.Delimiter.Should().BeNull("auto-detection matches the CLI default");
    }
}