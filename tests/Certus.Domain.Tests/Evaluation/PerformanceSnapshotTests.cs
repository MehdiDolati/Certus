using Certus.Domain.Evaluation.Entities;
using FluentAssertions;

namespace Certus.Domain.Tests.Evaluation;

public class PerformanceSnapshotTests
{
    [Fact]
    public void PerformanceSnapshot_Should_Store_All_Properties()
    {
        var id = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var strategyId = Guid.NewGuid();
        var date = new DateOnly(2026, 1, 15);

        var snapshot = new PerformanceSnapshot(
            id, reportId, portfolioId, strategyId, date,
            actualReturn: 0.02m, supposedReturn: 0.015m,
            dailyPnL: 500m, equity: 100000m,
            drawdown: 0.05m, sharpe: 1.2m);

        snapshot.Id.Should().Be(id);
        snapshot.ReportId.Should().Be(reportId);
        snapshot.PortfolioId.Should().Be(portfolioId);
        snapshot.StrategyId.Should().Be(strategyId);
        snapshot.Date.Should().Be(date);
        snapshot.ActualReturn.Should().Be(0.02m);
        snapshot.SupposedReturn.Should().Be(0.015m);
        snapshot.DailyPnL.Should().Be(500m);
        snapshot.Equity.Should().Be(100000m);
        snapshot.Drawdown.Should().Be(0.05m);
        snapshot.Sharpe.Should().Be(1.2m);
    }

    [Fact]
    public void PerformanceSnapshot_Should_Allow_Null_StrategyId()
    {
        var snapshot = new PerformanceSnapshot(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null,
            new DateOnly(2026, 1, 1),
            0.01m, 0.01m, 100m, 50000m, 0.02m, 0.8m);

        snapshot.StrategyId.Should().BeNull();
    }
}
