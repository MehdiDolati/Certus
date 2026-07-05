using Certus.Domain.Execution.ValueObjects;
using FluentAssertions;

namespace Certus.Domain.Tests.Execution;

public class TradeLifecycleTests
{
    [Fact]
    public void Constructor_Should_Set_All_Properties()
    {
        var signalTime = new DateTime(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc);
        var orderTime = new DateTime(2025, 1, 15, 10, 0, 1, DateTimeKind.Utc);
        var fillTime = new DateTime(2025, 1, 15, 10, 0, 2, DateTimeKind.Utc);
        var closeTime = new DateTime(2025, 1, 15, 10, 5, 0, DateTimeKind.Utc);

        var lifecycle = new TradeLifecycle(signalTime, orderTime, fillTime, closeTime);

        lifecycle.SignalTime.Should().Be(signalTime);
        lifecycle.OrderTime.Should().Be(orderTime);
        lifecycle.FillTime.Should().Be(fillTime);
        lifecycle.CloseTime.Should().Be(closeTime);
    }

    [Fact]
    public void Constructor_Should_Allow_Null_Optional_Times()
    {
        var lifecycle = new TradeLifecycle(DateTime.UtcNow, null, null, null);

        lifecycle.OrderTime.Should().BeNull();
        lifecycle.FillTime.Should().BeNull();
        lifecycle.CloseTime.Should().BeNull();
    }

    [Fact]
    public void SignalToOrder_Should_Calculate_Correctly()
    {
        var signalTime = new DateTime(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc);
        var orderTime = new DateTime(2025, 1, 15, 10, 0, 5, DateTimeKind.Utc);

        var lifecycle = new TradeLifecycle(signalTime, orderTime, null, null);

        lifecycle.SignalToOrder.Should().Be(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void SignalToOrder_Should_Be_Null_When_No_OrderTime()
    {
        var lifecycle = new TradeLifecycle(DateTime.UtcNow, null, null, null);

        lifecycle.SignalToOrder.Should().BeNull();
    }

    [Fact]
    public void OrderToFill_Should_Calculate_Correctly()
    {
        var signalTime = new DateTime(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc);
        var orderTime = new DateTime(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc);
        var fillTime = new DateTime(2025, 1, 15, 10, 0, 3, DateTimeKind.Utc);

        var lifecycle = new TradeLifecycle(signalTime, orderTime, fillTime, null);

        lifecycle.OrderToFill.Should().Be(TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void OrderToFill_Should_Be_Null_When_No_FillTime()
    {
        var lifecycle = new TradeLifecycle(DateTime.UtcNow, DateTime.UtcNow, null, null);

        lifecycle.OrderToFill.Should().BeNull();
    }

    [Fact]
    public void OrderToFill_Should_Be_Null_When_No_OrderTime()
    {
        var lifecycle = new TradeLifecycle(DateTime.UtcNow, null, DateTime.UtcNow, null);

        lifecycle.OrderToFill.Should().BeNull();
    }

    [Fact]
    public void TotalDuration_Should_Calculate_Correctly()
    {
        var signalTime = new DateTime(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc);
        var closeTime = new DateTime(2025, 1, 15, 10, 10, 0, DateTimeKind.Utc);

        var lifecycle = new TradeLifecycle(signalTime, null, null, closeTime);

        lifecycle.TotalDuration.Should().Be(TimeSpan.FromMinutes(10));
    }

    [Fact]
    public void TotalDuration_Should_Be_Null_When_No_CloseTime()
    {
        var lifecycle = new TradeLifecycle(DateTime.UtcNow, null, null, null);

        lifecycle.TotalDuration.Should().BeNull();
    }

    [Fact]
    public void TradeLifecycle_Should_Be_Equivalent_By_Value()
    {
        var time = new DateTime(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc);

        var l1 = new TradeLifecycle(time, time, time, time);
        var l2 = new TradeLifecycle(time, time, time, time);

        l1.Should().Be(l2);
    }
}
