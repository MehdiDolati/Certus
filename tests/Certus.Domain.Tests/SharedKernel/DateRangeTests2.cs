using Certus.Domain.SharedKernel;
using FluentAssertions;

namespace Certus.Domain.Tests.SharedKernel;

public class DateRangeTests2
{
    [Fact]
    public void DateRange_Should_Be_Created_With_Dates()
    {
        var from = new DateTime(2026, 1, 1);
        var to = new DateTime(2026, 12, 31);
        var range = new DateRange(from, to);
        range.From.Should().Be(from);
        range.To.Should().Be(to);
    }

    [Fact]
    public void Last30Days_Should_Be_Correct()
    {
        var range = DateRange.Last30Days;
        range.From.Should().BeCloseTo(DateTime.UtcNow.AddDays(-30), TimeSpan.FromSeconds(1));
        range.To.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Last90Days_Should_Be_Correct()
    {
        var range = DateRange.Last90Days;
        range.From.Should().BeCloseTo(DateTime.UtcNow.AddDays(-90), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void YearToDate_Should_Be_Correct()
    {
        var range = DateRange.YearToDate;
        range.From!.Value.Year.Should().Be(DateTime.UtcNow.Year);
        range.From.Value.Month.Should().Be(1);
        range.From.Value.Day.Should().Be(1);
    }

    [Fact]
    public void Last1Year_Should_Be_Correct()
    {
        var range = DateRange.Last1Year;
        range.From.Should().BeCloseTo(DateTime.UtcNow.AddYears(-1), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void All_Should_Have_Null_Dates()
    {
        var range = DateRange.All;
        range.From.Should().BeNull();
        range.To.Should().BeNull();
    }
}
