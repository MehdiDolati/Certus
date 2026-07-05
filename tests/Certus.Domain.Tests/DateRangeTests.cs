using Certus.Domain.SharedKernel;
using FluentAssertions;

namespace Certus.Domain.Tests;

public class DateRangeTests
{
    [Fact]
    public void DateRange_Last30Days_Should_Have_From_30_Days_Ago()
    {
        var range = DateRange.Last30Days;
        range.From.Should().BeCloseTo(DateTime.UtcNow.AddDays(-30), TimeSpan.FromSeconds(5));
        range.To.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void DateRange_All_Should_Have_Null_From_And_To()
    {
        var range = DateRange.All;
        range.From.Should().BeNull();
        range.To.Should().BeNull();
    }

    [Fact]
    public void DateRange_Equality_Should_Work_Correctly()
    {
        var range1 = new DateRange(null, null);
        var range2 = new DateRange(null, null);
        range1.Should().Be(range2);
    }
}
