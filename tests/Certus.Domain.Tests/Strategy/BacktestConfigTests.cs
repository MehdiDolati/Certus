using Certus.Domain.Market.ValueObjects;
using Certus.Domain.Market.ValueObjects;
using Certus.Domain.Strategy.ValueObjects;
using Certus.Domain.SharedKernel;
using FluentAssertions;

namespace Certus.Domain.Tests.Strategy;

public class BacktestConfigTests
{
    [Fact]
    public void Constructor_Should_Set_Properties_Correctly()
    {
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 12, 31);
        var capital = new Money(100_000m, Currency.USD);
        var symbols = new List<Symbol> { new("BTCUSD", AssetClass.Crypto) };
        var timeFrame = new TimeFrame("1h");

        var config = new BacktestConfig(startDate, endDate, capital, symbols, timeFrame);

        config.StartDate.Should().Be(startDate);
        config.EndDate.Should().Be(endDate);
        config.InitialCapital.Should().Be(capital);
        config.Symbols.Should().BeEquivalentTo(symbols);
        config.TimeFrame.Should().Be(timeFrame);
    }

    [Fact]
    public void Constructor_EndDate_Before_StartDate_Should_Throw()
    {
        var startDate = new DateTime(2024, 12, 31);
        var endDate = new DateTime(2024, 1, 1);

        var act = () => new BacktestConfig(
            startDate,
            endDate,
            new Money(100_000m, Currency.USD),
            new List<Symbol>(),
            new TimeFrame("1h"));

        act.Should().Throw<ArgumentException>()
            .WithMessage("*End date must be after start date*");
    }

    [Fact]
    public void Constructor_EndDate_Equals_StartDate_Should_Throw()
    {
        var date = new DateTime(2024, 6, 15);

        var act = () => new BacktestConfig(
            date,
            date,
            new Money(100_000m, Currency.USD),
            new List<Symbol>(),
            new TimeFrame("1h"));

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Duration_Should_Return_Correct_Timespan()
    {
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 12, 31);
        var config = new BacktestConfig(
            startDate,
            endDate,
            new Money(100_000m, Currency.USD),
            new List<Symbol>(),
            new TimeFrame("1h"));

        config.Duration.Should().Be(endDate - startDate);
    }

    [Fact]
    public void Record_Should_Be_Equal_When_Values_Are_Same()
    {
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 12, 31);
        var capital = new Money(100_000m, Currency.USD);
        var symbols = new List<Symbol> { new("BTCUSD", AssetClass.Crypto) };
        var timeFrame = new TimeFrame("1h");

        var config1 = new BacktestConfig(startDate, endDate, capital, symbols, timeFrame);
        var config2 = new BacktestConfig(startDate, endDate, capital, symbols, timeFrame);

        config1.Should().Be(config2);
    }

    [Fact]
    public void Record_Should_Be_Inequal_When_Values_Differ()
    {
        var symbols = new List<Symbol> { new("BTCUSD", AssetClass.Crypto) };
        var timeFrame = new TimeFrame("1h");

        var config1 = new BacktestConfig(
            new DateTime(2024, 1, 1),
            new DateTime(2024, 12, 31),
            new Money(100_000m, Currency.USD),
            symbols,
            timeFrame);
        var config2 = new BacktestConfig(
            new DateTime(2024, 1, 1),
            new DateTime(2024, 12, 31),
            new Money(200_000m, Currency.USD),
            symbols,
            timeFrame);

        config1.Should().NotBe(config2);
    }

    [Fact]
    public void Multiple_Symbols_Should_Be_Stored()
    {
        var symbols = new List<Symbol>
        {
            new("BTCUSD", AssetClass.Crypto),
            new("ETHUSD", AssetClass.Crypto),
            new("EURUSD", AssetClass.Forex)
        };

        var config = new BacktestConfig(
            new DateTime(2024, 1, 1),
            new DateTime(2024, 12, 31),
            new Money(100_000m, Currency.USD),
            symbols,
            new TimeFrame("1h"));

        config.Symbols.Should().HaveCount(3);
    }
}
