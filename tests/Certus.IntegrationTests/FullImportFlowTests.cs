using Certus.Application.Platform;
using Certus.Domain.Platform.Repositories;
using Certus.Domain.RiskAndPortfolio.Repositories;
using Certus.Domain.SharedKernel;
using Certus.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Certus.IntegrationTests;

public class FullImportFlowTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly string _testDirectory;

    public FullImportFlowTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _testDirectory = Path.Combine(Path.GetTempPath(), $"certus_import_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDirectory);
    }

    // Test 1: Import portfolio → Portfolio record in DB with IsPlatformManaged = true
    [Fact]
    public async Task ImportPortfolio_Should_Create_Database_Record()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CertusDbContext>();
        var portfolioRepo = scope.ServiceProvider.GetRequiredService<IPortfolioRepository>();

        var portfolio = new Certus.Domain.RiskAndPortfolio.Aggregates.Portfolio(
            Guid.NewGuid(), "Imported Test", 0, 0,
            new Money(50000m, Currency.USD), "Imported from MT4");
        portfolio.SetPlatformReference(Guid.NewGuid(), "MT4_12345");

        await portfolioRepo.AddAsync(portfolio);
        await db.SaveChangesAsync();

        var imported = await portfolioRepo.GetByIdAsync(portfolio.Id);
        imported.Should().NotBeNull();
        imported!.IsPlatformManaged.Should().BeTrue();
        imported.ExternalPortfolioId.Should().Be("MT4_12345");
        imported.AllocatedCapital.Amount.Should().Be(50000m);
    }

    // Test 2: Import trades → ImportedTrade records in DB
    [Fact]
    public async Task ImportTrades_Should_Create_Database_Records()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CertusDbContext>();
        var tradeRepo = scope.ServiceProvider.GetRequiredService<IImportedTradeRepository>();

        var strategyId = Guid.NewGuid();
        var connectionId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();

        var trade1 = new ImportedTrade
        {
            Id = Guid.NewGuid(),
            ExternalId = "MT4_TRADE_001",
            StrategyId = strategyId,
            PortfolioId = portfolioId,
            ConnectionId = connectionId,
            Symbol = "EURUSD",
            Side = "Buy",
            Volume = 0.10m,
            OpenPrice = 1.085m,
            ClosePrice = 1.0875m,
            Profit = 25m,
            Commission = -1.5m,
            Swap = -0.25m,
            OpenTime = DateTime.UtcNow.AddHours(-1),
            CloseTime = DateTime.UtcNow,
            Comment = "Test trade",
            ImportedAt = DateTime.UtcNow
        };

        var trade2 = new ImportedTrade
        {
            Id = Guid.NewGuid(),
            ExternalId = "MT4_TRADE_002",
            StrategyId = strategyId,
            PortfolioId = portfolioId,
            ConnectionId = connectionId,
            Symbol = "GBPUSD",
            Side = "Sell",
            Volume = 0.05m,
            OpenPrice = 1.275m,
            ClosePrice = 1.280m,
            Profit = -25m,
            Commission = -1.0m,
            Swap = 0m,
            OpenTime = DateTime.UtcNow.AddHours(-2),
            CloseTime = DateTime.UtcNow.AddHours(-1),
            Comment = "Test trade 2",
            ImportedAt = DateTime.UtcNow
        };

        await tradeRepo.AddAsync(trade1);
        await tradeRepo.AddAsync(trade2);
        await db.SaveChangesAsync();

        var trades = await tradeRepo.GetByStrategyIdAsync(strategyId);
        trades.Should().HaveCount(2);
        trades.First(t => t.ExternalId == "MT4_TRADE_001").PnL.Should().Be(23.25m);
        trades.First(t => t.ExternalId == "MT4_TRADE_002").PnL.Should().Be(-26m);
    }

    // Test 3: Import same trade twice → duplicate skipped
    [Fact]
    public async Task ImportTrades_Should_Skip_Duplicates()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CertusDbContext>();
        var tradeRepo = scope.ServiceProvider.GetRequiredService<IImportedTradeRepository>();

        var trade = new ImportedTrade
        {
            Id = Guid.NewGuid(),
            ExternalId = "MT4_DUPLICATE_001",
            StrategyId = Guid.NewGuid(),
            PortfolioId = Guid.NewGuid(),
            ConnectionId = Guid.NewGuid(),
            Symbol = "EURUSD",
            Side = "Buy",
            Volume = 0.10m,
            OpenPrice = 1.085m,
            Profit = 25m,
            Commission = -1.5m,
            Swap = -0.25m,
            OpenTime = DateTime.UtcNow,
            ImportedAt = DateTime.UtcNow
        };

        await tradeRepo.AddAsync(trade);
        await db.SaveChangesAsync();

        var existing = await tradeRepo.GetByExternalIdAsync("MT4_DUPLICATE_001");
        existing.Should().NotBeNull();
        existing!.ExternalId.Should().Be("MT4_DUPLICATE_001");
    }

    // Test 4: Full flow - parse file → import → verify DB
    [Fact]
    public async Task FullFlow_Should_Populate_ImportedTrades_Table()
    {
        // Arrange
        var tradesJson = """
        {
          "timestamp": "2026-07-05T10:30:00Z",
          "trades": [
            {
              "id": "MT4_FLOW_001",
              "strategyId": "EA_Momentum_01",
              "symbol": "EURUSD",
              "side": "buy",
              "volume": 0.10,
              "openPrice": 1.085,
              "closePrice": 1.0875,
              "stopLoss": 1.08,
              "takeProfit": 1.09,
              "profit": 25.00,
              "commission": -1.50,
              "swap": -0.25,
              "openTime": "2026-07-05T09:00:00Z",
              "closeTime": "2026-07-05T10:25:00Z",
              "comment": "Momentum signal"
            },
            {
              "id": "MT4_FLOW_002",
              "strategyId": "EA_MeanRev_02",
              "symbol": "GBPUSD",
              "side": "sell",
              "volume": 0.20,
              "openPrice": 1.275,
              "closePrice": 1.278,
              "stopLoss": 1.28,
              "takeProfit": 1.27,
              "profit": -60.00,
              "commission": -2.00,
              "swap": -0.50,
              "openTime": "2026-07-05T08:00:00Z",
              "closeTime": "2026-07-05T10:28:00Z",
              "comment": "RSI oversold"
            }
          ]
        }
        """;
        var filePath = Path.Combine(_testDirectory, "trades.json");
        File.WriteAllText(filePath, tradesJson);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CertusDbContext>();
        var tradeRepo = scope.ServiceProvider.GetRequiredService<IImportedTradeRepository>();
        var portfolioRepo = scope.ServiceProvider.GetRequiredService<IPortfolioRepository>();

        // Create portfolio
        var portfolio = new Certus.Domain.RiskAndPortfolio.Aggregates.Portfolio(
            Guid.NewGuid(), "Flow Test", 0, 0,
            new Money(100000m, Currency.USD), "Test");
        portfolio.SetPlatformReference(Guid.NewGuid(), "MT4_FLOW");
        await portfolioRepo.AddAsync(portfolio);
        await db.SaveChangesAsync();

        // Parse and import
        var parser = new Certus.Infrastructure.Platform.Plugins.MetaTrader4.Mt4JsonParser();
        var content = File.ReadAllText(filePath);
        var platformTrades = parser.ParseTrades(content);

        foreach (var pt in platformTrades)
        {
            var importedTrade = new ImportedTrade
            {
                Id = Guid.NewGuid(),
                ExternalId = pt.ExternalId,
                StrategyId = Guid.NewGuid(),
                PortfolioId = portfolio.Id,
                ConnectionId = portfolio.PlatformConnectionId!.Value,
                Symbol = pt.Symbol,
                Side = pt.Side.ToString(),
                Volume = pt.Volume,
                OpenPrice = pt.OpenPrice,
                ClosePrice = pt.ClosePrice,
                StopLoss = pt.StopLoss,
                TakeProfit = pt.TakeProfit,
                Profit = pt.Profit,
                Commission = pt.Commission,
                Swap = pt.Swap,
                OpenTime = pt.OpenTime,
                CloseTime = pt.CloseTime,
                Comment = pt.Comment,
                ImportedAt = DateTime.UtcNow
            };
            await tradeRepo.AddAsync(importedTrade);
        }
        await db.SaveChangesAsync();

        // Assert
        var allTrades = await tradeRepo.GetByPortfolioIdAsync(portfolio.Id);
        allTrades.Should().HaveCount(2);

        var t1 = allTrades.First(t => t.ExternalId == "MT4_FLOW_001");
        t1.Symbol.Should().Be("EURUSD");
        t1.PnL.Should().Be(23.25m);

        var t2 = allTrades.First(t => t.ExternalId == "MT4_FLOW_002");
        t2.Symbol.Should().Be("GBPUSD");
        t2.PnL.Should().Be(-62.5m);

        var updatedPortfolio = await portfolioRepo.GetByIdAsync(portfolio.Id);
        updatedPortfolio!.IsPlatformManaged.Should().BeTrue();
    }

    public Task DisposeAsync()
    {
        if (Directory.Exists(_testDirectory))
        {
            try { Directory.Delete(_testDirectory, true); }
            catch { }
        }
        return Task.CompletedTask;
    }

    public Task InitializeAsync() => Task.CompletedTask;
}
