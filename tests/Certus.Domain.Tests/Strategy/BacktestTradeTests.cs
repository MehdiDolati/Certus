using Certus.Domain.Strategy.Entities;
using FluentAssertions;

namespace Certus.Domain.Tests.Strategy;

public class BacktestTradeTests
{
    [Fact]
    public void Constructor_Should_Set_Properties_Correctly()
    {
        var id = Guid.NewGuid();
        var backtestRunId = Guid.NewGuid();
        var entryTime = new DateTime(2024, 6, 1, 10, 0, 0);

        var trade = new BacktestTrade(id, backtestRunId, "BTCUSD", "Long", entryTime, 50_000m, 0.5m);

        trade.Id.Should().Be(id);
        trade.BacktestRunId.Should().Be(backtestRunId);
        trade.Symbol.Should().Be("BTCUSD");
        trade.Side.Should().Be("Long");
        trade.EntryTime.Should().Be(entryTime);
        trade.EntryPrice.Should().Be(50_000m);
        trade.Quantity.Should().Be(0.5m);
        trade.ExitTime.Should().BeNull();
        trade.ExitPrice.Should().BeNull();
        trade.PnL.Should().Be(0m);
    }

    [Fact]
    public void Close_Long_Should_Calculate_Positive_PnL()
    {
        var trade = CreateLongTrade(entryPrice: 50_000m, quantity: 1m);

        trade.Close(DateTime.UtcNow, 55_000m);

        trade.PnL.Should().Be(5_000m);
    }

    [Fact]
    public void Close_Long_Should_Calculate_Negative_PnL()
    {
        var trade = CreateLongTrade(entryPrice: 50_000m, quantity: 1m);

        trade.Close(DateTime.UtcNow, 45_000m);

        trade.PnL.Should().Be(-5_000m);
    }

    [Fact]
    public void Close_Short_Should_Calculate_Positive_PnL()
    {
        var trade = CreateShortTrade(entryPrice: 50_000m, quantity: 1m);

        trade.Close(DateTime.UtcNow, 45_000m);

        trade.PnL.Should().Be(5_000m);
    }

    [Fact]
    public void Close_Short_Should_Calculate_Negative_PnL()
    {
        var trade = CreateShortTrade(entryPrice: 50_000m, quantity: 1m);

        trade.Close(DateTime.UtcNow, 55_000m);

        trade.PnL.Should().Be(-5_000m);
    }

    [Fact]
    public void Close_Should_Set_ExitTime()
    {
        var trade = CreateLongTrade();
        var exitTime = new DateTime(2024, 6, 1, 12, 0, 0);

        trade.Close(exitTime, 55_000m);

        trade.ExitTime.Should().Be(exitTime);
    }

    [Fact]
    public void Close_Should_Set_ExitPrice()
    {
        var trade = CreateLongTrade();

        trade.Close(DateTime.UtcNow, 55_000m);

        trade.ExitPrice.Should().Be(55_000m);
    }

    [Fact]
    public void Close_Long_Should_Calculate_Fractional_Quantity()
    {
        var trade = CreateLongTrade(entryPrice: 50_000m, quantity: 0.5m);

        trade.Close(DateTime.UtcNow, 52_000m);

        trade.PnL.Should().Be(1_000m);
    }

    private static BacktestTrade CreateLongTrade(
        decimal entryPrice = 50_000m,
        decimal quantity = 1m)
    {
        return new BacktestTrade(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "BTCUSD",
            "Long",
            DateTime.UtcNow,
            entryPrice,
            quantity);
    }

    private static BacktestTrade CreateShortTrade(
        decimal entryPrice = 50_000m,
        decimal quantity = 1m)
    {
        return new BacktestTrade(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "BTCUSD",
            "Short",
            DateTime.UtcNow,
            entryPrice,
            quantity);
    }
}
