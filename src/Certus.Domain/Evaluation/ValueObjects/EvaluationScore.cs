using Certus.Domain.SharedKernel;

namespace Certus.Domain.Evaluation.ValueObjects;

public enum ScoreTrend
{
    Improving = 0,
    Stable = 1,
    Declining = 2
}

public record EvaluationScore 
{
    public decimal Overall { get; }
    public decimal Consistency { get; }
    public decimal RiskAdjusted { get; }
    public ScoreTrend Trend { get; }

    public EvaluationScore(
        decimal overall,
        decimal consistency,
        decimal riskAdjusted,
        ScoreTrend trend)
    {
        Overall = overall;
        Consistency = consistency;
        RiskAdjusted = riskAdjusted;
        Trend = trend;
    }

    public bool IsExcellent => Overall >= 0.8m && RiskAdjusted >= 0.7m;
    public bool IsPoor => Overall < 0.4m || RiskAdjusted < 0.3m;
}
