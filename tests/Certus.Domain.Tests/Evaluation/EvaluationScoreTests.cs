using Certus.Domain.Evaluation.ValueObjects;
using FluentAssertions;

namespace Certus.Domain.Tests.Evaluation;

public class EvaluationScoreTests
{
    [Fact]
    public void EvaluationScore_Should_Store_All_Properties()
    {
        var score = new EvaluationScore(0.85m, 0.9m, 0.75m, ScoreTrend.Improving);

        score.Overall.Should().Be(0.85m);
        score.Consistency.Should().Be(0.9m);
        score.RiskAdjusted.Should().Be(0.75m);
        score.Trend.Should().Be(ScoreTrend.Improving);
    }

    [Theory]
    [InlineData(0.8, 0.7, true)]
    [InlineData(0.8, 0.6, false)]
    [InlineData(0.7, 0.7, false)]
    [InlineData(0.9, 0.8, true)]
    public void IsExcellent_Should_Return_Correctly(decimal overall, decimal riskAdjusted, bool expected)
    {
        var score = new EvaluationScore(overall, 0.5m, riskAdjusted, ScoreTrend.Stable);

        score.IsExcellent.Should().Be(expected);
    }

    [Theory]
    [InlineData(0.3, 0.3, true)]
    [InlineData(0.4, 0.2, true)]
    [InlineData(0.3, 0.4, true)]
    [InlineData(0.4, 0.3, false)]
    [InlineData(0.4, 0.4, false)]
    [InlineData(0.5, 0.5, false)]
    public void IsPoor_Should_Return_Correctly(decimal overall, decimal riskAdjusted, bool expected)
    {
        var score = new EvaluationScore(overall, 0.5m, riskAdjusted, ScoreTrend.Stable);

        score.IsPoor.Should().Be(expected);
    }

    [Fact]
    public void ScoreTrend_Should_Have_Correct_Values()
    {
        ((int)ScoreTrend.Improving).Should().Be(0);
        ((int)ScoreTrend.Stable).Should().Be(1);
        ((int)ScoreTrend.Declining).Should().Be(2);
    }

    [Fact]
    public void EvaluationScore_Should_Be_Equal_When_Values_Are_Same()
    {
        var a = new EvaluationScore(0.8m, 0.9m, 0.7m, ScoreTrend.Improving);
        var b = new EvaluationScore(0.8m, 0.9m, 0.7m, ScoreTrend.Improving);

        a.Should().Be(b);
    }

    [Fact]
    public void EvaluationScore_Should_Be_Unequal_When_Trend_Differs()
    {
        var a = new EvaluationScore(0.8m, 0.9m, 0.7m, ScoreTrend.Improving);
        var b = new EvaluationScore(0.8m, 0.9m, 0.7m, ScoreTrend.Declining);

        a.Should().NotBe(b);
    }
}
