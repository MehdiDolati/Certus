using System.Text.Json;
using Certus.Domain.Platform.Interfaces;
using Certus.Domain.Platform.ValueObjects;
using Certus.Infrastructure.Platform.Plugins.MetaTrader4;
using FluentAssertions;

namespace Certus.Infrastructure.Tests;

public class Mt4JsonParserTests
{
    private readonly Mt4JsonParser _sut = new();
    private readonly string _fixturesPath;

    public Mt4JsonParserTests()
    {
        _fixturesPath = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "tests", "fixtures"));
    }

    // AC-021: ParsePortfolio_Should_Match_Defining_Schema
    [Fact]
    public void ParsePortfolio_Should_Parse_Valid_Fixture()
    {
        var json = File.ReadAllText(Path.Combine(_fixturesPath, "portfolio_status.json"));

        var result = _sut.ParsePortfolio(json);

        result.Should().NotBeNull();
        result.ExternalId.Should().Be("12345");
        result.Name.Should().Be("Mehdi Dolati");
        result.Balance.Should().Be(100000.00m);
        result.Equity.Should().Be(102500.00m);
        result.Margin.Should().Be(15000.00m);
        result.FreeMargin.Should().Be(87500.00m);
        result.Profit.Should().Be(2500.00m);
        result.Strategies.Should().HaveCount(2);
    }

    [Fact]
    public void ParsePortfolio_Should_Parse_Strategy_Data()
    {
        var json = File.ReadAllText(Path.Combine(_fixturesPath, "portfolio_status.json"));

        var result = _sut.ParsePortfolio(json);

        var strategy = result.Strategies.First();
        strategy.ExternalId.Should().Be("EA_Momentum_01");
        strategy.Name.Should().Be("Momentum Strategy");
        strategy.IsActive.Should().BeTrue();
        strategy.Profit.Should().Be(1800.00m);
        strategy.TotalTrades.Should().Be(45);
    }

    [Fact]
    public void ParsePortfolio_Should_Handle_Missing_Fields()
    {
        var json = File.ReadAllText(Path.Combine(_fixturesPath, "portfolio_status_missing_fields.json"));

        var result = _sut.ParsePortfolio(json);

        result.Should().NotBeNull();
        result.ExternalId.Should().Be("12345");
        result.Balance.Should().Be(100000.00m);
        result.Strategies.Should().BeEmpty();
    }

    [Fact]
    public void ParsePortfolio_Should_Throw_On_Invalid_Json()
    {
        var act = () => _sut.ParsePortfolio("{ invalid json }");
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void ParsePortfolio_Should_Throw_On_Null_Portfolio()
    {
        var json = """{"timestamp": "2026-07-05T10:30:00Z", "portfolio": null}""";
        var act = () => _sut.ParsePortfolio(json);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ParsePortfolio_Should_Throw_On_Empty_Data()
    {
        var act = () => _sut.ParsePortfolio("");
        act.Should().Throw<JsonException>();
    }

    // AC-022: ParseTrades_Should_Match_Defining_Schema
    [Fact]
    public void ParseTrades_Should_Parse_Valid_Fixture()
    {
        var json = File.ReadAllText(Path.Combine(_fixturesPath, "trades.json"));

        var result = _sut.ParseTrades(json);

        result.Should().HaveCount(3);
    }

    [Fact]
    public void ParseTrades_Should_Parse_Trade_Details()
    {
        var json = File.ReadAllText(Path.Combine(_fixturesPath, "trades.json"));

        var result = _sut.ParseTrades(json);

        var trade = result.First();
        trade.ExternalId.Should().Be("789012");
        trade.StrategyExternalId.Should().Be("EA_Momentum_01");
        trade.Symbol.Should().Be("EURUSD");
        trade.Side.Should().Be(TradeSide.Buy);
        trade.Volume.Should().Be(0.10m);
        trade.OpenPrice.Should().Be(1.0850m);
        trade.ClosePrice.Should().Be(1.0875m);
        trade.Profit.Should().Be(25.00m);
        trade.Commission.Should().Be(-1.50m);
        trade.Swap.Should().Be(-0.25m);
        trade.Comment.Should().Be("Momentum signal");
    }

    [Fact]
    public void ParseTrades_Should_Handle_Empty_Trades()
    {
        var json = File.ReadAllText(Path.Combine(_fixturesPath, "trades_empty.json"));

        var result = _sut.ParseTrades(json);

        result.Should().BeEmpty();
    }

    [Fact]
    public void ParseTrades_Should_Handle_Null_Trades()
    {
        var json = """{"timestamp": "2026-07-05T10:30:00Z"}""";

        var result = _sut.ParseTrades(json);

        result.Should().BeEmpty();
    }

    [Fact]
    public void ParseTrades_Should_Throw_On_Invalid_Json()
    {
        var act = () => _sut.ParseTrades("{ invalid }");
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void ParseTrade_Should_Parse_Single_Trade()
    {
        var json = """{"externalId": "123", "strategyExternalId": "EA_01", "symbol": "EURUSD", "side": "Buy", "volume": 0.1, "openPrice": 1.085, "profit": 25, "commission": -1.5, "swap": -0.25, "openTime": "2026-07-05T09:00:00Z"}""";

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };
        var result = JsonSerializer.Deserialize<PlatformTrade>(json, options)!;

        result.ExternalId.Should().Be("123");
    }

    [Fact]
    public void ParseStrategy_Should_Parse_Single_Strategy()
    {
        var json = """{"externalId": "EA_01", "name": "Test", "isActive": true, "profit": 100, "totalTrades": 10}""";

        var result = _sut.ParseStrategy(json);

        result.ExternalId.Should().Be("EA_01");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public void FormatId_Should_Be_Json()
    {
        _sut.FormatId.Should().Be("json");
    }

    [Fact]
    public void SupportedExtensions_Should_Include_Json()
    {
        _sut.SupportedExtensions.Should().Contain(".json");
    }
}
