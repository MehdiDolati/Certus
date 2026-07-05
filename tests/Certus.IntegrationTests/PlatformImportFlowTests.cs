using Certus.Infrastructure.Platform.Plugins.MetaTrader4;
using FluentAssertions;

namespace Certus.IntegrationTests;

public class PlatformImportFlowTests
{
    private readonly Mt4JsonParser _parser = new();
    private readonly string _testDirectory;

    public PlatformImportFlowTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"certus_platform_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDirectory);
    }

    [Fact]
    public void FullImportFlow_Should_Parse_And_Extract_All_Data()
    {
        // Arrange: Create a realistic portfolio_status.json
        var portfolioJson = CreatePortfolioStatusJson();
        var filePath = Path.Combine(_testDirectory, "portfolio_status.json");
        File.WriteAllText(filePath, portfolioJson);

        // Act: Parse the file
        var content = File.ReadAllText(filePath);
        var portfolio = _parser.ParsePortfolio(content);

        // Assert: All data extracted correctly
        portfolio.Should().NotBeNull();
        portfolio.ExternalId.Should().Be("54321");
        portfolio.Name.Should().Be("Test Account");
        portfolio.Balance.Should().Be(50000m);
        portfolio.Equity.Should().Be(51200m);
        portfolio.Strategies.Should().HaveCount(2);

        var activeStrategy = portfolio.Strategies.First(s => s.IsActive);
        activeStrategy.ExternalId.Should().Be("EA_Trend_01");
        activeStrategy.TotalTrades.Should().Be(25);

        var inactiveStrategy = portfolio.Strategies.First(s => !s.IsActive);
        inactiveStrategy.ExternalId.Should().Be("EA_Scalper_02");
    }

    [Fact]
    public void TradeImportFlow_Should_Parse_And_Calculate_PnL()
    {
        // Arrange: Create a realistic trades.json
        var tradesJson = CreateTradesJson();
        var filePath = Path.Combine(_testDirectory, "trades.json");
        File.WriteAllText(filePath, tradesJson);

        // Act: Parse the file
        var content = File.ReadAllText(filePath);
        var trades = _parser.ParseTrades(content);

        // Assert: All trades parsed with correct PnL
        trades.Should().HaveCount(3);

        var winningTrade = trades.First(t => t.Profit > 0);
        winningTrade.Symbol.Should().Be("EURUSD");
        winningTrade.Side.Should().Be(Domain.Platform.ValueObjects.TradeSide.Buy);
        winningTrade.Profit.Should().Be(150m);

        var losingTrade = trades.First(t => t.Profit < 0);
        losingTrade.Symbol.Should().Be("GBPUSD");
        losingTrade.Profit.Should().Be(-75m);

        // Verify each trade has valid data
        foreach (var trade in trades)
        {
            trade.ExternalId.Should().NotBeNullOrEmpty();
            trade.Symbol.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public void FileWatcher_Should_Detect_New_File()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "new_portfolio.json");
        var fileChanged = false;

        var watcher = new FileSystemWatcher(_testDirectory, "*.json");
        watcher.Created += (s, e) => fileChanged = true;
        watcher.EnableRaisingEvents = true;

        // Act: Create the file
        File.WriteAllText(filePath, """{"test": true}""");

        // Wait for event
        Thread.Sleep(200);

        // Assert
        fileChanged.Should().BeTrue();
        watcher.Dispose();
    }

    [Fact]
    public void FileWatcher_Should_Detect_File_Modification()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "update_test.json");
        File.WriteAllText(filePath, """{"version": 1}""");
        var fileChanged = false;

        var watcher = new FileSystemWatcher(_testDirectory, "*.json");
        watcher.Changed += (s, e) => fileChanged = true;
        watcher.EnableRaisingEvents = true;

        // Act: Modify the file
        Thread.Sleep(100);
        File.WriteAllText(filePath, """{"version": 2}""");

        // Wait for event
        Thread.Sleep(200);

        // Assert
        fileChanged.Should().BeTrue();
        watcher.Dispose();
    }

    [Fact]
    public void MultipleFiles_Should_Be_Independent()
    {
        // Arrange & Act
        var portfolioJson = """{"timestamp":"2026-07-05T10:00:00Z","portfolio":{"id":"1","name":"A","balance":100}}""";
        var tradesJson = """{"timestamp":"2026-07-05T10:00:00Z","trades":[]}""";

        File.WriteAllText(Path.Combine(_testDirectory, "p1.json"), portfolioJson);
        File.WriteAllText(Path.Combine(_testDirectory, "t1.json"), tradesJson);

        // Act & Assert: Parse each independently
        var p = _parser.ParsePortfolio(File.ReadAllText(Path.Combine(_testDirectory, "p1.json")));
        var t = _parser.ParseTrades(File.ReadAllText(Path.Combine(_testDirectory, "t1.json")));

        p.ExternalId.Should().Be("1");
        t.Should().BeEmpty();
    }

    private static string CreatePortfolioStatusJson()
    {
        return """
        {
          "timestamp": "2026-07-05T10:30:00Z",
          "portfolio": {
            "id": "54321",
            "name": "Test Account",
            "balance": 50000.00,
            "equity": 51200.00,
            "margin": 8000.00,
            "freeMargin": 43200.00,
            "profit": 1200.00
          },
          "strategies": [
            {
              "id": "EA_Trend_01",
              "name": "Trend Following",
              "active": true,
              "profit": 800.00,
              "totalTrades": 25,
              "lastTradeTime": "2026-07-05T10:20:00Z"
            },
            {
              "id": "EA_Scalper_02",
              "name": "Scalper",
              "active": false,
              "profit": 400.00,
              "totalTrades": 100,
              "lastTradeTime": "2026-07-04T18:00:00Z"
            }
          ]
        }
        """;
    }

    private static string CreateTradesJson()
    {
        return """
        {
          "timestamp": "2026-07-05T10:30:00Z",
          "trades": [
            {
              "id": "100001",
              "strategyId": "EA_Trend_01",
              "symbol": "EURUSD",
              "side": "buy",
              "volume": 0.20,
              "openPrice": 1.0800,
              "closePrice": 1.0875,
              "stopLoss": 1.0750,
              "takeProfit": 1.0900,
              "profit": 150.00,
              "commission": -3.00,
              "swap": -0.50,
              "openTime": "2026-07-05T08:00:00Z",
              "closeTime": "2026-07-05T10:20:00Z",
              "comment": "Trend entry"
            },
            {
              "id": "100002",
              "strategyId": "EA_Trend_01",
              "symbol": "GBPUSD",
              "side": "sell",
              "volume": 0.15,
              "openPrice": 1.2750,
              "closePrice": 1.2800,
              "stopLoss": 1.2800,
              "takeProfit": 1.2700,
              "profit": -75.00,
              "commission": -2.25,
              "swap": 0.00,
              "openTime": "2026-07-05T09:00:00Z",
              "closeTime": "2026-07-05T10:15:00Z",
              "comment": "Counter-trend stop"
            },
            {
              "id": "100003",
              "strategyId": "EA_Scalper_02",
              "symbol": "USDJPY",
              "side": "buy",
              "volume": 0.10,
              "openPrice": 145.50,
              "closePrice": 145.75,
              "stopLoss": 145.20,
              "takeProfit": 146.00,
              "profit": 25.00,
              "commission": -1.00,
              "swap": -0.10,
              "openTime": "2026-07-05T10:00:00Z",
              "closeTime": "2026-07-05T10:28:00Z",
              "comment": "Scalp"
            }
          ]
        }
        """;
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            try { Directory.Delete(_testDirectory, true); }
            catch { /* cleanup best effort */ }
        }
    }
}
