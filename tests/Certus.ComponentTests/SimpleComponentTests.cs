using Bunit;
using FluentAssertions;

namespace Certus.ComponentTests;

public class SimpleComponentTests : TestContext
{
    [Fact]
    public void TestContext_Should_Be_Created_Successfully()
    {
        var context = new TestContext();
        context.Should().NotBeNull();
        context.Services.Should().NotBeNull();
    }
}
