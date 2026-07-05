using Certus.Application.Portfolios.DTOs;
using Certus.Application.Strategies.DTOs;
using Certus.Application.Trading.DTOs;
using Certus.Application.Platform.DTOs;
using Certus.Domain.RiskAndPortfolio.Enums;
using Certus.Domain.Strategy.Enums;
using Certus.Domain.Execution.Enums;
using Certus.Domain.Platform.Enums;
using Certus.Domain.SharedKernel;
using FluentAssertions;

namespace Certus.Application.Tests;

public class DtoTests
{
    // === PortfolioDetailDto ===

    [Fact]
    public void PortfolioDetailDto_Should_Store_All_Properties()
    {
        var summary = new PortfolioDto(Guid.NewGuid(), "Fund", "Desc", PortfolioStatus.Active,
            0.1m, 0.08m, 0.02m, 1.5m, -0.05m, 10, 0.6m, 1_000_000m);
        var strategies = new List<StrategyPerformanceDto>();
        var equityCurve = new List<PerformanceSnapshotDto>();
        var drawdownCurve = new List<PerformanceSnapshotDto>();
        var monthlyReturns = new List<MonthlyReturnDto>();

        var dto = new PortfolioDetailDto(summary, strategies, equityCurve, drawdownCurve, monthlyReturns);

        dto.Summary.Should().Be(summary);
        dto.Strategies.Should().BeSameAs(strategies);
        dto.EquityCurve.Should().BeSameAs(equityCurve);
        dto.DrawdownCurve.Should().BeSameAs(drawdownCurve);
        dto.MonthlyReturns.Should().BeSameAs(monthlyReturns);
    }

    [Fact]
    public void PortfolioDetailDto_Should_Be_Equal_When_Same_References()
    {
        var summary = new PortfolioDto(Guid.NewGuid(), "Fund", "Desc", PortfolioStatus.Active,
            0.1m, 0.08m, 0.02m, 1.5m, -0.05m, 10, 0.6m, 1_000_000m);
        var strategies = new List<StrategyPerformanceDto>();
        var equityCurve = new List<PerformanceSnapshotDto>();
        var drawdownCurve = new List<PerformanceSnapshotDto>();
        var monthlyReturns = new List<MonthlyReturnDto>();

        var dto1 = new PortfolioDetailDto(summary, strategies, equityCurve, drawdownCurve, monthlyReturns);
        var dto2 = new PortfolioDetailDto(summary, strategies, equityCurve, drawdownCurve, monthlyReturns);

        dto1.Should().Be(dto2);
    }

    // === DashboardKpis ===

    [Fact]
    public void DashboardKpis_Should_Store_All_Properties()
    {
        var dto = new DashboardKpis(5_000_000m, 250_000m, 1.8m, -0.08m, 0.65m, 3);

        dto.TotalAum.Should().Be(5_000_000m);
        dto.TotalPnl.Should().Be(250_000m);
        dto.AvgSharpe.Should().Be(1.8m);
        dto.MaxDrawdown.Should().Be(-0.08m);
        dto.WinRate.Should().Be(0.65m);
        dto.ActiveCount.Should().Be(3);
    }

    [Fact]
    public void DashboardKpis_Should_Be_Equal_When_Same_Values()
    {
        var dto1 = new DashboardKpis(1m, 2m, 3m, 4m, 5m, 6);
        var dto2 = new DashboardKpis(1m, 2m, 3m, 4m, 5m, 6);

        dto1.Should().Be(dto2);
    }

    [Fact]
    public void DashboardKpis_Should_Be_Different_When_Values_Differ()
    {
        var dto1 = new DashboardKpis(1m, 2m, 3m, 4m, 5m, 6);
        var dto2 = new DashboardKpis(1m, 2m, 3m, 4m, 5m, 7);

        dto1.Should().NotBe(dto2);
    }

    // === PlatformDashboardDto ===

    [Fact]
    public void PlatformDashboardDto_Should_Have_Default_Values()
    {
        var dto = new PlatformDashboardDto();

        dto.TotalConnections.Should().Be(0);
        dto.ActiveConnections.Should().Be(0);
        dto.TotalPortfolios.Should().Be(0);
        dto.TotalStrategies.Should().Be(0);
        dto.TotalTrades.Should().Be(0);
        dto.TotalPnL.Should().Be(0m);
        dto.Connections.Should().BeEmpty();
    }

    [Fact]
    public void PlatformDashboardDto_Should_Store_All_Properties()
    {
        var connections = new List<PlatformConnectionDto>
        {
            new() { Id = Guid.NewGuid(), PlatformName = "MT4" }
        };
        var dto = new PlatformDashboardDto
        {
            TotalConnections = 5,
            ActiveConnections = 3,
            TotalPortfolios = 10,
            TotalStrategies = 20,
            TotalTrades = 150,
            TotalPnL = 50_000m,
            Connections = connections
        };

        dto.TotalConnections.Should().Be(5);
        dto.ActiveConnections.Should().Be(3);
        dto.TotalPortfolios.Should().Be(10);
        dto.TotalStrategies.Should().Be(20);
        dto.TotalTrades.Should().Be(150);
        dto.TotalPnL.Should().Be(50_000m);
        dto.Connections.Should().HaveCount(1);
    }

    // === PlatformStatusDto ===

    [Fact]
    public void PlatformStatusDto_Should_Have_Default_Values()
    {
        var dto = new PlatformStatusDto();

        dto.ConnectionId.Should().Be(Guid.Empty);
        dto.PlatformName.Should().Be(string.Empty);
        dto.Status.Should().Be(default(PlatformConnectionStatus));
        dto.IsConnected.Should().BeFalse();
        dto.ConnectedAt.Should().BeNull();
        dto.LastDataReceivedAt.Should().BeNull();
        dto.ImportedPortfolios.Should().Be(0);
        dto.ImportedStrategies.Should().Be(0);
        dto.ImportedTrades.Should().Be(0);
        dto.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void PlatformStatusDto_Should_Store_All_Properties()
    {
        var now = DateTime.UtcNow;
        var dto = new PlatformStatusDto
        {
            ConnectionId = Guid.NewGuid(),
            PlatformName = "MetaTrader 4",
            Status = PlatformConnectionStatus.Connected,
            IsConnected = true,
            ConnectedAt = now,
            LastDataReceivedAt = now.AddMinutes(-5),
            ImportedPortfolios = 3,
            ImportedStrategies = 12,
            ImportedTrades = 500,
            ErrorMessage = null
        };

        dto.PlatformName.Should().Be("MetaTrader 4");
        dto.Status.Should().Be(PlatformConnectionStatus.Connected);
        dto.IsConnected.Should().BeTrue();
        dto.ConnectedAt.Should().Be(now);
        dto.ImportedPortfolios.Should().Be(3);
        dto.ImportedStrategies.Should().Be(12);
        dto.ImportedTrades.Should().Be(500);
    }

    // === PlatformStrategyDto ===

    [Fact]
    public void PlatformStrategyDto_Should_Have_Default_Values()
    {
        var dto = new PlatformStrategyDto();

        dto.ExternalId.Should().Be(string.Empty);
        dto.Name.Should().Be(string.Empty);
        dto.IsActive.Should().BeFalse();
        dto.Profit.Should().Be(0m);
        dto.TotalTrades.Should().Be(0);
        dto.WinningTrades.Should().Be(0);
        dto.LosingTrades.Should().Be(0);
        dto.LastTradeTime.Should().BeNull();
        dto.Timestamp.Should().Be(default(DateTime));
    }

    [Fact]
    public void PlatformStrategyDto_Should_Store_All_Properties()
    {
        var now = DateTime.UtcNow;
        var dto = new PlatformStrategyDto
        {
            ExternalId = "EA_001",
            Name = "Momentum Pro",
            IsActive = true,
            Profit = 15_000m,
            TotalTrades = 200,
            WinningTrades = 130,
            LosingTrades = 70,
            LastTradeTime = now,
            Timestamp = now
        };

        dto.ExternalId.Should().Be("EA_001");
        dto.Name.Should().Be("Momentum Pro");
        dto.IsActive.Should().BeTrue();
        dto.Profit.Should().Be(15_000m);
        dto.TotalTrades.Should().Be(200);
        dto.WinningTrades.Should().Be(130);
        dto.LosingTrades.Should().Be(70);
    }

    // === MonthlyReturnDto ===

    [Fact]
    public void MonthlyReturnDto_Should_Store_All_Properties()
    {
        var dto = new MonthlyReturnDto(2024, 6, 3.5m);

        dto.Year.Should().Be(2024);
        dto.Month.Should().Be(6);
        dto.ReturnPercent.Should().Be(3.5m);
    }

    [Fact]
    public void MonthlyReturnDto_Should_Be_Equal_When_Same_Values()
    {
        var dto1 = new MonthlyReturnDto(2024, 1, 2.5m);
        var dto2 = new MonthlyReturnDto(2024, 1, 2.5m);

        dto1.Should().Be(dto2);
    }

    [Fact]
    public void MonthlyReturnDto_Should_Be_Different_When_Month_Differs()
    {
        var dto1 = new MonthlyReturnDto(2024, 1, 2.5m);
        var dto2 = new MonthlyReturnDto(2024, 2, 2.5m);

        dto1.Should().NotBe(dto2);
    }

    // === PerformanceSnapshotDto ===

    [Fact]
    public void PerformanceSnapshotDto_Should_Store_All_Properties()
    {
        var dto = new PerformanceSnapshotDto(
            new DateOnly(2024, 6, 15), 0.12m, 0.10m, 5_000m, 105_000m, -0.03m, 1.8m);

        dto.Date.Should().Be(new DateOnly(2024, 6, 15));
        dto.ActualReturn.Should().Be(0.12m);
        dto.SupposedReturn.Should().Be(0.10m);
        dto.DailyPnl.Should().Be(5_000m);
        dto.Equity.Should().Be(105_000m);
        dto.Drawdown.Should().Be(-0.03m);
        dto.Sharpe.Should().Be(1.8m);
    }

    [Fact]
    public void PerformanceSnapshotDto_Should_Be_Equal_When_Same_Values()
    {
        var dto1 = new PerformanceSnapshotDto(new DateOnly(2024, 1, 1), 1m, 2m, 3m, 4m, 5m, 6m);
        var dto2 = new PerformanceSnapshotDto(new DateOnly(2024, 1, 1), 1m, 2m, 3m, 4m, 5m, 6m);

        dto1.Should().Be(dto2);
    }

    // === TradeDetailDto ===

    [Fact]
    public void TradeDetailDto_Should_Store_All_Properties()
    {
        var trade = new TradeDto(
            Guid.NewGuid(), "Momentum", "BTCUSD", TradeSide.Long,
            DateTime.UtcNow.AddHours(-1), DateTime.UtcNow,
            40_000m, 42_000m, 0.5m, 1_000m, 2.5m,
            TimeSpan.FromHours(1), 10m, 2m, TradeStatus.Closed);

        var dto = new TradeDetailDto(trade, "Breakout signal", 500m, 200m);

        dto.Trade.Should().Be(trade);
        dto.AgentReason.Should().Be("Breakout signal");
        dto.PortfolioImpact.Should().Be(500m);
        dto.StrategyImpact.Should().Be(200m);
    }

    [Fact]
    public void TradeDetailDto_Should_Be_Equal_When_Same_Values()
    {
        var trade = new TradeDto(
            Guid.NewGuid(), "Test", "ETHUSD", TradeSide.Short,
            DateTime.UtcNow, null, 3000m, null, 1m, 0m, 0m,
            null, 0m, 0m, TradeStatus.Open);

        var dto1 = new TradeDetailDto(trade, "Reason", 1m, 2m);
        var dto2 = new TradeDetailDto(trade, "Reason", 1m, 2m);

        dto1.Should().Be(dto2);
    }

    // === TradeDto ===

    [Fact]
    public void TradeDto_Should_Store_All_Properties()
    {
        var entryTime = new DateTime(2024, 6, 15, 10, 0, 0, DateTimeKind.Utc);
        var exitTime = new DateTime(2024, 6, 15, 14, 0, 0, DateTimeKind.Utc);
        var duration = exitTime - entryTime;

        var dto = new TradeDto(
            Guid.NewGuid(), "Arbitrage", "SOLUSDT", TradeSide.Short,
            entryTime, exitTime, 150m, 140m, 100m, 1_000m, 6.67m,
            duration, 5m, 1m, TradeStatus.Closed);

        dto.StrategyName.Should().Be("Arbitrage");
        dto.Symbol.Should().Be("SOLUSDT");
        dto.Side.Should().Be(TradeSide.Short);
        dto.EntryTime.Should().Be(entryTime);
        dto.ExitTime.Should().Be(exitTime);
        dto.EntryPrice.Should().Be(150m);
        dto.ExitPrice.Should().Be(140m);
        dto.Quantity.Should().Be(100m);
        dto.PnL.Should().Be(1_000m);
        dto.PnLPercent.Should().Be(6.67m);
        dto.Duration.Should().Be(duration);
        dto.Fees.Should().Be(5m);
        dto.Slippage.Should().Be(1m);
        dto.Status.Should().Be(TradeStatus.Closed);
    }

    [Fact]
    public void TradeDto_Should_Be_Equal_When_Same_Values()
    {
        var id = Guid.NewGuid();
        var time = DateTime.UtcNow;
        var dto1 = new TradeDto(id, "A", "BTC", TradeSide.Long, time, null, 100m, null, 1m, 0m, 0m, null, 0m, 0m, TradeStatus.Open);
        var dto2 = new TradeDto(id, "A", "BTC", TradeSide.Long, time, null, 100m, null, 1m, 0m, 0m, null, 0m, 0m, TradeStatus.Open);

        dto1.Should().Be(dto2);
    }

    [Fact]
    public void TradeDto_Should_Be_Different_When_Values_Differ()
    {
        var id = Guid.NewGuid();
        var dto1 = new TradeDto(id, "A", "BTC", TradeSide.Long, DateTime.UtcNow, null, 100m, null, 1m, 0m, 0m, null, 0m, 0m, TradeStatus.Open);
        var dto2 = new TradeDto(id, "B", "ETH", TradeSide.Short, DateTime.UtcNow, null, 200m, null, 2m, 0m, 0m, null, 0m, 0m, TradeStatus.Closed);

        dto1.Should().NotBe(dto2);
    }

    // === StrategyPerformanceDto ===

    [Fact]
    public void StrategyPerformanceDto_Should_Store_All_Properties()
    {
        var dto = new StrategyPerformanceDto(
            Guid.NewGuid(), "TrendFollower", "TrendFollowing", 30m,
            0.15m, 0.12m, 0.03m, 1.8m, 50, 0.6m, StrategyStatus.Active);

        dto.Name.Should().Be("TrendFollower");
        dto.Type.Should().Be("TrendFollowing");
        dto.ContributionPercent.Should().Be(30m);
        dto.ActualReturn.Should().Be(0.15m);
        dto.SupposedReturn.Should().Be(0.12m);
        dto.Variance.Should().Be(0.03m);
        dto.Sharpe.Should().Be(1.8m);
        dto.TradeCount.Should().Be(50);
        dto.WinRate.Should().Be(0.6m);
        dto.Status.Should().Be(StrategyStatus.Active);
    }

    [Fact]
    public void StrategyPerformanceDto_Should_Be_Equal_When_Same_Values()
    {
        var id = Guid.NewGuid();
        var dto1 = new StrategyPerformanceDto(id, "A", "Momentum", 10m, 1m, 2m, 3m, 4m, 5, 6m, StrategyStatus.Draft);
        var dto2 = new StrategyPerformanceDto(id, "A", "Momentum", 10m, 1m, 2m, 3m, 4m, 5, 6m, StrategyStatus.Draft);

        dto1.Should().Be(dto2);
    }

    [Fact]
    public void StrategyPerformanceDto_Should_Be_Different_When_Values_Differ()
    {
        var id = Guid.NewGuid();
        var dto1 = new StrategyPerformanceDto(id, "A", "Momentum", 10m, 1m, 2m, 3m, 4m, 5, 6m, StrategyStatus.Draft);
        var dto2 = new StrategyPerformanceDto(id, "B", "Arbitrage", 20m, 2m, 3m, 4m, 5m, 6, 7m, StrategyStatus.Active);

        dto1.Should().NotBe(dto2);
    }

    // === PortfolioDto ===

    [Fact]
    public void PortfolioDto_Should_Store_All_Properties()
    {
        var id = Guid.NewGuid();
        var dto = new PortfolioDto(id, "Alpha Fund", "Description", PortfolioStatus.Active,
            0.15m, 0.12m, 0.03m, 1.8m, -0.05m, 25, 0.64m, 2_500_000m);

        dto.Id.Should().Be(id);
        dto.Name.Should().Be("Alpha Fund");
        dto.Description.Should().Be("Description");
        dto.Status.Should().Be(PortfolioStatus.Active);
        dto.ActualReturn.Should().Be(0.15m);
        dto.SupposedReturn.Should().Be(0.12m);
        dto.Variance.Should().Be(0.03m);
        dto.Sharpe.Should().Be(1.8m);
        dto.MaxDrawdown.Should().Be(-0.05m);
        dto.TradeCount.Should().Be(25);
        dto.WinRate.Should().Be(0.64m);
        dto.Aum.Should().Be(2_500_000m);
    }

    // === PortfolioFilter ===

    [Fact]
    public void PortfolioFilter_Should_Have_Correct_Defaults()
    {
        var filter = new PortfolioFilter();

        filter.Status.Should().BeNull();
        filter.MinAum.Should().BeNull();
        filter.Search.Should().BeNull();
        filter.SortBy.Should().Be(PortfolioSortBy.Variance);
        filter.Ascending.Should().BeFalse();
        filter.Period.Should().BeNull();
    }

    [Fact]
    public void PortfolioFilter_Should_Store_All_Properties()
    {
        var period = new DateRange(new DateTime(2024, 1, 1), new DateTime(2024, 12, 31));
        var filter = new PortfolioFilter(
            Status: PortfolioStatus.Active,
            MinAum: 1_000_000m,
            Search: "test",
            SortBy: PortfolioSortBy.Sharpe,
            Ascending: true,
            Period: period);

        filter.Status.Should().Be(PortfolioStatus.Active);
        filter.MinAum.Should().Be(1_000_000m);
        filter.Search.Should().Be("test");
        filter.SortBy.Should().Be(PortfolioSortBy.Sharpe);
        filter.Ascending.Should().BeTrue();
        filter.Period.Should().Be(period);
    }

    // === TradeFilter ===

    [Fact]
    public void TradeFilter_Should_Have_Correct_Defaults()
    {
        var filter = new TradeFilter();

        filter.PortfolioId.Should().BeNull();
        filter.StrategyId.Should().BeNull();
        filter.Symbol.Should().BeNull();
        filter.Side.Should().BeNull();
        filter.Status.Should().BeNull();
        filter.Period.Should().BeNull();
        filter.Search.Should().BeNull();
        filter.Page.Should().Be(1);
        filter.PageSize.Should().Be(20);
    }

    [Fact]
    public void TradeFilter_Should_Store_All_Properties()
    {
        var portfolioId = Guid.NewGuid();
        var strategyId = Guid.NewGuid();
        var period = new DateRange(new DateTime(2024, 1, 1), new DateTime(2024, 6, 30));

        var filter = new TradeFilter(
            PortfolioId: portfolioId,
            StrategyId: strategyId,
            Symbol: "BTCUSD",
            Side: TradeSide.Long,
            Status: TradeStatus.Closed,
            Period: period,
            Search: "profit",
            Page: 3,
            PageSize: 50);

        filter.PortfolioId.Should().Be(portfolioId);
        filter.StrategyId.Should().Be(strategyId);
        filter.Symbol.Should().Be("BTCUSD");
        filter.Side.Should().Be(TradeSide.Long);
        filter.Status.Should().Be(TradeStatus.Closed);
        filter.Period.Should().Be(period);
        filter.Search.Should().Be("profit");
        filter.Page.Should().Be(3);
        filter.PageSize.Should().Be(50);
    }

    // === CreatePortfolioRequest ===

    [Fact]
    public void CreatePortfolioRequest_Should_Store_All_Properties()
    {
        var dto = new CreatePortfolioRequest("Fund", "Desc", 0.15m, 1.5m, 1_000_000m);

        dto.Name.Should().Be("Fund");
        dto.Description.Should().Be("Desc");
        dto.TargetReturn.Should().Be(0.15m);
        dto.TargetSharpe.Should().Be(1.5m);
        dto.AllocatedCapital.Should().Be(1_000_000m);
    }

    // === UpdatePortfolioRequest ===

    [Fact]
    public void UpdatePortfolioRequest_Should_Store_All_Properties()
    {
        var dto = new UpdatePortfolioRequest("New Fund", "New Desc", 0.2m, 2.0m, 2_000_000m);

        dto.Name.Should().Be("New Fund");
        dto.Description.Should().Be("New Desc");
        dto.TargetReturn.Should().Be(0.2m);
        dto.TargetSharpe.Should().Be(2.0m);
        dto.AllocatedCapital.Should().Be(2_000_000m);
    }

    // === CreateStrategyRequest ===

    [Fact]
    public void CreateStrategyRequest_Should_Store_All_Properties()
    {
        var portfolioId = Guid.NewGuid();
        var dto = new CreateStrategyRequest(portfolioId, "Momentum", "Momentum", 0.25m, 0.6m);

        dto.PortfolioId.Should().Be(portfolioId);
        dto.Name.Should().Be("Momentum");
        dto.Type.Should().Be("Momentum");
        dto.TargetReturn.Should().Be(0.25m);
        dto.Weight.Should().Be(0.6m);
    }

    // === UpdateStrategyRequest ===

    [Fact]
    public void UpdateStrategyRequest_Should_Store_All_Properties()
    {
        var dto = new UpdateStrategyRequest("Arbitrage", "Arbitrage", 0.3m, 0.7m);

        dto.Name.Should().Be("Arbitrage");
        dto.Type.Should().Be("Arbitrage");
        dto.TargetReturn.Should().Be(0.3m);
        dto.Weight.Should().Be(0.7m);
    }

    // === PlatformConnectionDto ===

    [Fact]
    public void PlatformConnectionDto_Should_Have_Default_Values()
    {
        var dto = new PlatformConnectionDto();

        dto.Id.Should().Be(Guid.Empty);
        dto.PlatformId.Should().Be(string.Empty);
        dto.PlatformName.Should().Be(string.Empty);
        dto.Status.Should().Be(default(PlatformConnectionStatus));
        dto.ConnectedAt.Should().BeNull();
        dto.LastDataReceivedAt.Should().BeNull();
        dto.ErrorMessage.Should().BeNull();
        dto.RetryCount.Should().Be(0);
    }

    [Fact]
    public void PlatformConnectionDto_Should_Store_All_Properties()
    {
        var now = DateTime.UtcNow;
        var dto = new PlatformConnectionDto
        {
            Id = Guid.NewGuid(),
            PlatformId = "metatrader4",
            PlatformName = "MetaTrader 4",
            Status = PlatformConnectionStatus.Connected,
            ConnectedAt = now,
            LastDataReceivedAt = now.AddMinutes(-2),
            ErrorMessage = null,
            RetryCount = 0
        };

        dto.PlatformId.Should().Be("metatrader4");
        dto.PlatformName.Should().Be("MetaTrader 4");
        dto.Status.Should().Be(PlatformConnectionStatus.Connected);
        dto.ConnectedAt.Should().Be(now);
        dto.RetryCount.Should().Be(0);
    }

    // === PlatformPortfolioDto ===

    [Fact]
    public void PlatformPortfolioDto_Should_Have_Default_Values()
    {
        var dto = new PlatformPortfolioDto();

        dto.ExternalId.Should().Be(string.Empty);
        dto.Name.Should().Be(string.Empty);
        dto.Balance.Should().Be(0m);
        dto.Equity.Should().Be(0m);
        dto.Margin.Should().Be(0m);
        dto.FreeMargin.Should().Be(0m);
        dto.Profit.Should().Be(0m);
        dto.CertusPortfolioId.Should().BeNull();
        dto.Strategies.Should().BeEmpty();
    }

    [Fact]
    public void PlatformPortfolioDto_Should_Store_All_Properties()
    {
        var certusId = Guid.NewGuid();
        var strategies = new List<PlatformStrategySummaryDto>
        {
            new() { ExternalId = "EA_01", Name = "Momentum", IsActive = true }
        };

        var dto = new PlatformPortfolioDto
        {
            ExternalId = "MT4_123",
            Name = "Main Account",
            Balance = 100_000m,
            Equity = 105_000m,
            Margin = 20_000m,
            FreeMargin = 85_000m,
            Profit = 5_000m,
            CertusPortfolioId = certusId,
            Strategies = strategies
        };

        dto.ExternalId.Should().Be("MT4_123");
        dto.Name.Should().Be("Main Account");
        dto.Balance.Should().Be(100_000m);
        dto.Equity.Should().Be(105_000m);
        dto.Margin.Should().Be(20_000m);
        dto.FreeMargin.Should().Be(85_000m);
        dto.Profit.Should().Be(5_000m);
        dto.CertusPortfolioId.Should().Be(certusId);
        dto.Strategies.Should().HaveCount(1);
    }

    // === PlatformStrategySummaryDto ===

    [Fact]
    public void PlatformStrategySummaryDto_Should_Have_Default_Values()
    {
        var dto = new PlatformStrategySummaryDto();

        dto.ExternalId.Should().Be(string.Empty);
        dto.Name.Should().Be(string.Empty);
        dto.IsActive.Should().BeFalse();
        dto.Profit.Should().Be(0m);
        dto.TotalTrades.Should().Be(0);
        dto.LastTradeTime.Should().BeNull();
    }

    [Fact]
    public void PlatformStrategySummaryDto_Should_Store_All_Properties()
    {
        var now = DateTime.UtcNow;
        var dto = new PlatformStrategySummaryDto
        {
            ExternalId = "EA_01",
            Name = "Momentum",
            IsActive = true,
            Profit = 5_000m,
            TotalTrades = 100,
            LastTradeTime = now
        };

        dto.ExternalId.Should().Be("EA_01");
        dto.Name.Should().Be("Momentum");
        dto.IsActive.Should().BeTrue();
        dto.Profit.Should().Be(5_000m);
        dto.TotalTrades.Should().Be(100);
        dto.LastTradeTime.Should().Be(now);
    }

    // === ImportTradesResult ===

    [Fact]
    public void ImportTradesResult_Should_Have_Default_Values()
    {
        var dto = new ImportTradesResult();

        dto.TradesImported.Should().Be(0);
        dto.TradesSkipped.Should().Be(0);
        dto.TotalPnL.Should().Be(0m);
        dto.EarliestTrade.Should().BeNull();
        dto.LatestTrade.Should().BeNull();
        dto.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ImportTradesResult_Should_Store_All_Properties()
    {
        var earliest = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var latest = new DateTime(2024, 6, 30, 23, 59, 59, DateTimeKind.Utc);

        var dto = new ImportTradesResult
        {
            TradesImported = 50,
            TradesSkipped = 5,
            TotalPnL = 2_500m,
            EarliestTrade = earliest,
            LatestTrade = latest,
            Errors = new List<string> { "Trade 123 skipped" }
        };

        dto.TradesImported.Should().Be(50);
        dto.TradesSkipped.Should().Be(5);
        dto.TotalPnL.Should().Be(2_500m);
        dto.EarliestTrade.Should().Be(earliest);
        dto.LatestTrade.Should().Be(latest);
        dto.Errors.Should().HaveCount(1);
    }

    // === ConnectPlatformRequest ===

    [Fact]
    public void ConnectPlatformRequest_Should_Have_Correct_Defaults()
    {
        var dto = new ConnectPlatformRequest();

        dto.PlatformId.Should().Be(string.Empty);
        dto.Name.Should().BeNull();
        dto.FilePath.Should().BeNull();
        dto.ServerAddress.Should().BeNull();
        dto.Port.Should().BeNull();
        dto.ApiKey.Should().BeNull();
        dto.UseFileWatcher.Should().BeTrue();
    }

    [Fact]
    public void ConnectPlatformRequest_Should_Store_All_Properties()
    {
        var dto = new ConnectPlatformRequest
        {
            PlatformId = "metatrader4",
            Name = "My MT4",
            FilePath = @"C:\Data\portfolio.json",
            ServerAddress = "127.0.0.1",
            Port = 443,
            ApiKey = "abc123",
            UseFileWatcher = false
        };

        dto.PlatformId.Should().Be("metatrader4");
        dto.Name.Should().Be("My MT4");
        dto.FilePath.Should().Be(@"C:\Data\portfolio.json");
        dto.ServerAddress.Should().Be("127.0.0.1");
        dto.Port.Should().Be(443);
        dto.ApiKey.Should().Be("abc123");
        dto.UseFileWatcher.Should().BeFalse();
    }
}
