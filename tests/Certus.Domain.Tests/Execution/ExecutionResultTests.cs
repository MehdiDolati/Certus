using Certus.Domain.Execution.Enums;
using Certus.Domain.Execution.ValueObjects;
using FluentAssertions;

namespace Certus.Domain.Tests.Execution;

public class ExecutionResultTests
{
    [Fact]
    public void Constructor_Should_Set_All_Properties()
    {
        var orderId = Guid.NewGuid();

        var result = new ExecutionResult(orderId, OrderStatus.Filled, 100m, 50000m, 5m, 0.2m, "warning");

        result.OrderId.Should().Be(orderId);
        result.Status.Should().Be(OrderStatus.Filled);
        result.FilledQuantity.Should().Be(100m);
        result.AverageFillPrice.Should().Be(50000m);
        result.Fees.Should().Be(5m);
        result.Slippage.Should().Be(0.2m);
        result.ErrorMessage.Should().Be("warning");
    }

    [Fact]
    public void Constructor_Should_Allow_Null_ErrorMessage()
    {
        var result = new ExecutionResult(Guid.NewGuid(), OrderStatus.Filled, 100m, 50000m, 0m, 0m);

        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void IsSuccessful_Should_Be_True_For_Filled()
    {
        var result = new ExecutionResult(Guid.NewGuid(), OrderStatus.Filled, 100m, 50000m, 0m, 0m);

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void IsSuccessful_Should_Be_True_For_PartiallyFilled()
    {
        var result = new ExecutionResult(Guid.NewGuid(), OrderStatus.PartiallyFilled, 50m, 50000m, 0m, 0m);

        result.IsSuccessful.Should().BeTrue();
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.Submitted)]
    [InlineData(OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Rejected)]
    public void IsSuccessful_Should_Be_False_For_Non_Filled_Statuses(OrderStatus status)
    {
        var result = new ExecutionResult(Guid.NewGuid(), status, 0m, 0m, 0m, 0m);

        result.IsSuccessful.Should().BeFalse();
    }

    [Fact]
    public void ExecutionResult_Should_Be_Equivalent_By_Value()
    {
        var orderId = Guid.NewGuid();
        var r1 = new ExecutionResult(orderId, OrderStatus.Filled, 100m, 50000m, 5m, 0.2m);
        var r2 = new ExecutionResult(orderId, OrderStatus.Filled, 100m, 50000m, 5m, 0.2m);

        r1.Should().Be(r2);
    }
}
