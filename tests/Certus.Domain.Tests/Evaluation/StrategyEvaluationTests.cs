using Certus.Domain.Evaluation.Entities;
using Certus.Domain.Evaluation.ValueObjects;
using FluentAssertions;

namespace Certus.Domain.Tests.Evaluation;

public class StrategyEvaluationTests
{
    [Fact]
    public void StrategyEvaluation_Should_Store_All_Properties()
    {
        var id = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        var strategyId = Guid.NewGuid();
        var score = new EvaluationScore(0.85m, 0.9m, 0.75m, ScoreTrend.Improving);

        var evaluation = new StrategyEvaluation(id, reportId, strategyId, score, "Increase allocation");

        evaluation.Id.Should().Be(id);
        evaluation.ReportId.Should().Be(reportId);
        evaluation.StrategyId.Should().Be(strategyId);
        evaluation.Score.Should().Be(score);
        evaluation.Recommendation.Should().Be("Increase allocation");
        evaluation.EvaluatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void StrategyEvaluation_Should_Set_EvaluatedAt_To_UtcNow()
    {
        var before = DateTime.UtcNow;

        var evaluation = new StrategyEvaluation(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            new EvaluationScore(0.5m, 0.5m, 0.5m, ScoreTrend.Stable),
            "Hold");

        var after = DateTime.UtcNow;

        evaluation.EvaluatedAt.Should().BeOnOrAfter(before);
        evaluation.EvaluatedAt.Should().BeOnOrBefore(after);
    }
}
