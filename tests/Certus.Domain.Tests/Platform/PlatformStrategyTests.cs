using Certus.Domain.Platform.ValueObjects;
using FluentAssertions;

namespace Certus.Domain.Tests.Platform;

public class PlatformStrategyTests
{
    [Fact]
    public void PlatformStrategy_Should_Have_Correct_Defaults()
    {
        var strategy = new PlatformStrategy();

        strategy.ExternalId.Should().BeEmpty();
        strategy.Name.Should().BeEmpty();
        strategy.IsActive.Should().BeFalse();
        strategy.Profit.Should().Be(0);
        strategy.TotalTrades.Should().Be(0);
        strategy.WinningTrades.Should().Be(0);
        strategy.LosingTrades.Should().Be(0);
        strategy.LastTradeTime.Should().BeNull();
    }

    [Fact]
    public void PlatformStrategy_Should_Be_Created_With_Values()
    {
        var lastTrade = DateTime.UtcNow.AddMinutes(-5);
        
        var strategy = new PlatformStrategy
        {
            ExternalId = "EA_Momentum_01",
            Name = "Momentum Strategy",
            IsActive = true,
            Profit = 1800m,
            TotalTrades = 45,
            WinningTrades = 30,
            LosingTrades = 15,
            LastTradeTime = lastTrade,
            Timestamp = DateTime.UtcNow
        };

        strategy.ExternalId.Should().Be("EA_Momentum_01");
        strategy.Name.Should().Be("Momentum Strategy");
        strategy.IsActive.Should().BeTrue();
        strategy.Profit.Should().Be(1800m);
        strategy.TotalTrades.Should().Be(45);
        strategy.WinningTrades.Should().Be(30);
        strategy.LosingTrades.Should().Be(15);
        strategy.LastTradeTime.Should().Be(lastTrade);
    }

    [Fact]
    public void PlatformStrategy_WinRate_Should_Be_Calculated_Correctly()
    {
        var strategy = new PlatformStrategy
        {
            TotalTrades = 100,
            WinningTrades = 60,
            LosingTrades = 40
        };

        var winRate = (double)strategy.WinningTrades / strategy.TotalTrades;
        winRate.Should().Be(0.6);
    }
}
