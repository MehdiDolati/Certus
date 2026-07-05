using Certus.Domain.Evaluation.Enums;
using Certus.Domain.Evaluation.Events;
using Certus.Domain.Evaluation.ValueObjects;
using FluentAssertions;

namespace Certus.Domain.Tests.Evaluation;

public class DomainEventTests
{
    [Fact]
    public void PerformanceEvaluated_Should_Store_All_Properties()
    {
        var reportId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var returnMetrics = new ReturnMetrics(0.15m, 0.18m, [], []);
        var riskMetrics = new RiskMetrics(1.5m, 2.0m, 0.15m, 1.8m, 0.12m, 0.05m);

        var evt = new PerformanceEvaluated
        {
            ReportId = reportId,
            PortfolioId = portfolioId,
            ReturnMetrics = returnMetrics,
            RiskMetrics = riskMetrics
        };

        evt.ReportId.Should().Be(reportId);
        evt.PortfolioId.Should().Be(portfolioId);
        evt.ReturnMetrics.Should().Be(returnMetrics);
        evt.RiskMetrics.Should().Be(riskMetrics);
        evt.EventId.Should().NotBe(Guid.Empty);
        evt.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void StrategyScored_Should_Store_All_Properties()
    {
        var strategyId = Guid.NewGuid();
        var score = new EvaluationScore(0.85m, 0.9m, 0.75m, ScoreTrend.Improving);

        var evt = new StrategyScored
        {
            StrategyId = strategyId,
            Score = score,
            Recommendation = "Increase allocation"
        };

        evt.StrategyId.Should().Be(strategyId);
        evt.Score.Should().Be(score);
        evt.Recommendation.Should().Be("Increase allocation");
        evt.EventId.Should().NotBe(Guid.Empty);
        evt.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void StrategyScored_Should_Default_Recommendation_To_Empty()
    {
        var evt = new StrategyScored
        {
            StrategyId = Guid.NewGuid(),
            Score = new EvaluationScore(0.5m, 0.5m, 0.5m, ScoreTrend.Stable)
        };

        evt.Recommendation.Should().Be(string.Empty);
    }

    [Fact]
    public void DrawdownAlert_Should_Store_All_Properties()
    {
        var portfolioId = Guid.NewGuid();

        var evt = new DrawdownAlert
        {
            PortfolioId = portfolioId,
            CurrentDrawdown = 0.25m,
            Threshold = 0.2m,
            Message = "Drawdown exceeds threshold"
        };

        evt.PortfolioId.Should().Be(portfolioId);
        evt.CurrentDrawdown.Should().Be(0.25m);
        evt.Threshold.Should().Be(0.2m);
        evt.Message.Should().Be("Drawdown exceeds threshold");
        evt.EventId.Should().NotBe(Guid.Empty);
        evt.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void DrawdownAlert_Should_Default_Message_To_Empty()
    {
        var evt = new DrawdownAlert
        {
            PortfolioId = Guid.NewGuid(),
            CurrentDrawdown = 0.3m,
            Threshold = 0.2m
        };

        evt.Message.Should().Be(string.Empty);
    }

    [Fact]
    public void FeedbackGenerated_Should_Store_All_Properties()
    {
        var strategyId = Guid.NewGuid();

        var evt = new FeedbackGenerated
        {
            StrategyId = strategyId,
            FeedbackType = FeedbackType.Warning,
            Message = "Win rate declining"
        };

        evt.StrategyId.Should().Be(strategyId);
        evt.FeedbackType.Should().Be(FeedbackType.Warning);
        evt.Message.Should().Be("Win rate declining");
        evt.EventId.Should().NotBe(Guid.Empty);
        evt.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void All_Events_Should_Have_Different_EventIds()
    {
        var perf = new PerformanceEvaluated { ReportId = Guid.NewGuid(), PortfolioId = Guid.NewGuid(), ReturnMetrics = new ReturnMetrics(), RiskMetrics = new RiskMetrics(0, 0, 0, 0, 0, 0) };
        var scored = new StrategyScored { StrategyId = Guid.NewGuid(), Score = new EvaluationScore(0, 0, 0, ScoreTrend.Stable) };
        var drawdown = new DrawdownAlert { PortfolioId = Guid.NewGuid(), CurrentDrawdown = 0, Threshold = 0 };
        var feedback = new FeedbackGenerated { StrategyId = Guid.NewGuid(), FeedbackType = FeedbackType.Positive, Message = "" };

        var ids = new[] { perf.EventId, scored.EventId, drawdown.EventId, feedback.EventId };

        ids.Distinct().Should().HaveCount(4);
    }
}
