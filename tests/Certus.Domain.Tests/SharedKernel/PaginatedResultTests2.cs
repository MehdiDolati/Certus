using Certus.Domain.SharedKernel;
using FluentAssertions;

namespace Certus.Domain.Tests.SharedKernel;

public class PaginatedResultTests2
{
    [Fact]
    public void TotalPages_Should_Be_Calculated_Correctly()
    {
        var result = new PaginatedResult<string>([], 10, 1, 3);
        result.TotalPages.Should().Be(4);
    }

    [Fact]
    public void TotalPages_Should_Round_Up()
    {
        var result = new PaginatedResult<string>([], 10, 1, 4);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public void HasPrevious_Should_Be_False_For_First_Page()
    {
        var result = new PaginatedResult<string>([], 10, 1, 3);
        result.HasPrevious.Should().BeFalse();
    }

    [Fact]
    public void HasPrevious_Should_Be_True_For_Non_First_Page()
    {
        var result = new PaginatedResult<string>([], 10, 2, 3);
        result.HasPrevious.Should().BeTrue();
    }

    [Fact]
    public void HasNext_Should_Be_False_For_Last_Page()
    {
        var result = new PaginatedResult<string>([], 9, 3, 3);
        result.HasNext.Should().BeFalse();
    }

    [Fact]
    public void HasNext_Should_Be_True_For_Non_Last_Page()
    {
        var result = new PaginatedResult<string>([], 10, 1, 3);
        result.HasNext.Should().BeTrue();
    }
}
