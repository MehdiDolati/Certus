using Certus.Domain.Evaluation.Aggregates;
using Certus.Domain.Evaluation.Entities;
using Certus.Domain.Evaluation.Enums;
using Certus.Domain.Evaluation.ValueObjects;
using FluentAssertions;

namespace Certus.Domain.Tests.Evaluation;

public class PerformanceReportTests
{
    private static ReturnMetrics CreateReturnMetrics() =>
        new(0.15m, 0.18m, [0.02m, 0.03m], [0.001m, -0.002m]);

    private static RiskMetrics CreateRiskMetrics() =>
        new(1.5m, 2.0m, 0.15m, 1.8m, 0.12m, 0.05m);

    private static TradeMetrics CreateTradeMetrics() =>
        new(100, 0.6m, 2.0m, 500m, 250m, 150m);

    private static EvaluationScore CreateScore() =>
        new(0.85m, 0.9m, 0.75m, ScoreTrend.Improving);

    [Fact]
    public void PerformanceReport_Should_Be_Created_With_Correct_Properties()
    {
        var id = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var strategyId = Guid.NewGuid();
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 3, 31);
        var returnMetrics = CreateReturnMetrics();
        var riskMetrics = CreateRiskMetrics();
        var tradeMetrics = CreateTradeMetrics();
        var score = CreateScore();

        var report = new PerformanceReport(
            id, portfolioId, strategyId, EvaluationPeriod.Quarterly,
            startDate, endDate, returnMetrics, riskMetrics, tradeMetrics, score);

        report.Id.Should().Be(id);
        report.PortfolioId.Should().Be(portfolioId);
        report.StrategyId.Should().Be(strategyId);
        report.Period.Should().Be(EvaluationPeriod.Quarterly);
        report.StartDate.Should().Be(startDate);
        report.EndDate.Should().Be(endDate);
        report.ReturnMetrics.Should().Be(returnMetrics);
        report.RiskMetrics.Should().Be(riskMetrics);
        report.TradeMetrics.Should().Be(tradeMetrics);
        report.Score.Should().Be(score);
        report.GeneratedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void PerformanceReport_Should_Allow_Null_StrategyId()
    {
        var report = CreateReport(strategyId: null);

        report.StrategyId.Should().BeNull();
    }

    [Fact]
    public void AddSnapshot_Should_Add_To_Collection()
    {
        var report = CreateReport();
        var snapshot = new PerformanceSnapshot(
            Guid.NewGuid(), report.Id, Guid.NewGuid(), null,
            new DateOnly(2026, 1, 1), 0.02m, 0.015m, 500m, 100000m, 0.05m, 1.2m);

        report.AddSnapshot(snapshot);

        report.Snapshots.Should().ContainSingle().Which.Should().Be(snapshot);
    }

    [Fact]
    public void AddSnapshot_Should_Maintain_Insertion_Order()
    {
        var report = CreateReport();
        var snapshots = Enumerable.Range(0, 5)
            .Select(i => new PerformanceSnapshot(
                Guid.NewGuid(), report.Id, Guid.NewGuid(), null,
                new DateOnly(2026, 1, i + 1), 0.01m, 0.01m, 100m, 50000m, 0.01m, 1.0m))
            .ToList();

        foreach (var s in snapshots)
            report.AddSnapshot(s);

        report.Snapshots.Should().HaveCount(5);
        report.Snapshots.Should().Equal(snapshots);
    }

    [Fact]
    public void AddBenchmark_Should_Add_To_Collection()
    {
        var report = CreateReport();
        var benchmark = new BenchmarkComparison(
            Guid.NewGuid(), report.Id, BenchmarkType.MarketIndex,
            new BenchmarkResult(0.1m, 0.05m, 0.85m, 1.2m, 0.03m));

        report.AddBenchmark(benchmark);

        report.Benchmarks.Should().ContainSingle().Which.Should().Be(benchmark);
    }

    [Fact]
    public void AddEvaluation_Should_Add_To_Collection()
    {
        var report = CreateReport();
        var evaluation = new StrategyEvaluation(
            Guid.NewGuid(), report.Id, Guid.NewGuid(), CreateScore(), "Hold");

        report.AddEvaluation(evaluation);

        report.Evaluations.Should().ContainSingle().Which.Should().Be(evaluation);
    }

    [Fact]
    public void AddFeedback_Should_Add_To_Collection()
    {
        var report = CreateReport();
        var feedback = new FeedbackEntry(
            Guid.NewGuid(), report.Id, FeedbackType.Positive, "Good job");

        report.AddFeedback(feedback);

        report.Feedback.Should().ContainSingle().Which.Should().Be(feedback);
    }

    [Fact]
    public void UpdateMetrics_Should_Replace_All_Metrics()
    {
        var report = CreateReport();
        var newReturnMetrics = new ReturnMetrics(0.25m, 0.3m, [0.05m], []);
        var newRiskMetrics = new RiskMetrics(2.0m, 2.5m, 0.1m, 2.0m, 0.1m, 0.03m);
        var newTradeMetrics = new TradeMetrics(200, 0.7m, 3.0m, 600m, 200m, 250m);
        var newScore = new EvaluationScore(0.9m, 0.95m, 0.85m, ScoreTrend.Improving);

        report.UpdateMetrics(newReturnMetrics, newRiskMetrics, newTradeMetrics, newScore);

        report.ReturnMetrics.Should().Be(newReturnMetrics);
        report.RiskMetrics.Should().Be(newRiskMetrics);
        report.TradeMetrics.Should().Be(newTradeMetrics);
        report.Score.Should().Be(newScore);
        report.GeneratedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void UpdateMetrics_Should_Update_GeneratedAt()
    {
        var report = CreateReport();
        var originalGeneratedAt = report.GeneratedAt;

        Thread.Sleep(10);
        report.UpdateMetrics(CreateReturnMetrics(), CreateRiskMetrics(), CreateTradeMetrics(), CreateScore());

        report.GeneratedAt.Should().BeOnOrAfter(originalGeneratedAt);
    }

    [Fact]
    public void PerformanceReport_Collections_Should_Be_Empty_By_Default()
    {
        var report = CreateReport();

        report.Snapshots.Should().BeEmpty();
        report.Benchmarks.Should().BeEmpty();
        report.Evaluations.Should().BeEmpty();
        report.Feedback.Should().BeEmpty();
    }

    [Fact]
    public void DomainEvents_Should_Be_Empty_By_Default()
    {
        var report = CreateReport();

        report.DomainEvents.Should().BeEmpty();
    }

    private static PerformanceReport CreateReport(Guid? strategyId = null) =>
        new(
            Guid.NewGuid(), Guid.NewGuid(), strategyId, EvaluationPeriod.Monthly,
            new DateTime(2026, 1, 1), new DateTime(2026, 1, 31),
            CreateReturnMetrics(), CreateRiskMetrics(), CreateTradeMetrics(), CreateScore());
}
