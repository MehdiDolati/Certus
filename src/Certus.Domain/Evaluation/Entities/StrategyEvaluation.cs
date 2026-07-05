using Certus.Domain.Evaluation.ValueObjects;
using Certus.Domain.SharedKernel;

namespace Certus.Domain.Evaluation.Entities;

public class StrategyEvaluation : Entity
{
    public Guid ReportId { get; private set; }
    public Guid StrategyId { get; private set; }
    public EvaluationScore Score { get; private set; }
    public string Recommendation { get; private set; } = string.Empty;
    public DateTime EvaluatedAt { get; private set; }

    private StrategyEvaluation() { }

    public StrategyEvaluation(
        Guid id,
        Guid reportId,
        Guid strategyId,
        EvaluationScore score,
        string recommendation) : base(id)
    {
        ReportId = reportId;
        StrategyId = strategyId;
        Score = score;
        Recommendation = recommendation;
        EvaluatedAt = DateTime.UtcNow;
    }
}
