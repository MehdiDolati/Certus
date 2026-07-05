using Certus.Domain.Execution.Entities;
using Certus.Domain.SharedKernel;
using FluentAssertions;

namespace Certus.Domain.Tests.Execution;

public class FillTests
{
    [Fact]
    public void Constructor_Should_Set_All_Properties()
    {
        var id = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var filledAt = new DateTime(2025, 6, 15, 10, 30, 0, DateTimeKind.Utc);

        var fill = new Fill(id, orderId, "EX-FILL-1", 50000m, 10m, 5m, filledAt);

        fill.Id.Should().Be(id);
        fill.OrderId.Should().Be(orderId);
        fill.ExchangeFillId.Should().Be("EX-FILL-1");
        fill.Price.Should().Be(50000m);
        fill.Quantity.Should().Be(10m);
        fill.Fees.Should().Be(5m);
        fill.FilledAt.Should().Be(filledAt);
    }

    [Fact]
    public void Constructor_Should_Allow_Zero_Fees()
    {
        var fill = new Fill(Guid.NewGuid(), Guid.NewGuid(), "F-1", 100m, 1m, 0m, DateTime.UtcNow);

        fill.Fees.Should().Be(0m);
    }
}
