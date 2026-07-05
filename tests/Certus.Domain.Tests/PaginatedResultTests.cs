using Certus.Domain.SharedKernel;
using FluentAssertions;

namespace Certus.Domain.Tests;

public class PaginatedResultTests
{
    [Fact]
    public void PaginatedResult_TotalPages_Should_Calculate_Correctly()
    {
        var result = new PaginatedResult<string>([], 25, 1, 10);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public void PaginatedResult_TotalPages_Should_Round_Up()
    {
        var result = new PaginatedResult<string>([], 11, 1, 10);
        result.TotalPages.Should().Be(2);
    }

    [Fact]
    public void PaginatedResult_HasPrevious_Should_Be_False_For_First_Page()
    {
        var result = new PaginatedResult<string>([], 25, 1, 10);
        result.HasPrevious.Should().BeFalse();
    }

    [Fact]
    public void PaginatedResult_HasPrevious_Should_Be_True_For_Non_First_Page()
    {
        var result = new PaginatedResult<string>([], 25, 2, 10);
        result.HasPrevious.Should().BeTrue();
    }

    [Fact]
    public void PaginatedResult_HasNext_Should_Be_False_For_Last_Page()
    {
        var result = new PaginatedResult<string>([], 25, 3, 10);
        result.HasNext.Should().BeFalse();
    }

    [Fact]
    public void PaginatedResult_HasNext_Should_Be_True_For_Non_Last_Page()
    {
        var result = new PaginatedResult<string>([], 25, 1, 10);
        result.HasNext.Should().BeTrue();
    }
}
