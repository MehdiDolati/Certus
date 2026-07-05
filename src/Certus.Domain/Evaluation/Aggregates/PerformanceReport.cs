using Certus.Domain.Evaluation.Entities;
using Certus.Domain.Evaluation.Enums;
using Certus.Domain.Evaluation.ValueObjects;
using Certus.Domain.SharedKernel;

namespace Certus.Domain.Evaluation.Aggregates;

public class PerformanceReport : AggregateRoot
{
    private readonly List<PerformanceSnapshot> _snapshots = [];
    private readonly List<BenchmarkComparison> _benchmarks = [];
    private readonly List<StrategyEvaluation> _evaluations = [];
    private readonly List<FeedbackEntry> _feedback = [];

    public IReadOnlyList<PerformanceSnapshot> Snapshots => _snapshots.AsReadOnly();
    public IReadOnlyList<BenchmarkComparison> Benchmarks => _benchmarks.AsReadOnly();
    public IReadOnlyList<StrategyEvaluation> Evaluations => _evaluations.AsReadOnly();
    public IReadOnlyList<FeedbackEntry> Feedback => _feedback.AsReadOnly();

    public Guid PortfolioId { get; private set; }
    public Guid? StrategyId { get; private set; }
    public EvaluationPeriod Period { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public ReturnMetrics ReturnMetrics { get; private set; }
    public RiskMetrics RiskMetrics { get; private set; }
    public TradeMetrics TradeMetrics { get; private set; }
    public EvaluationScore Score { get; private set; }
    public DateTime GeneratedAt { get; private set; }

    private PerformanceReport() { }

    public PerformanceReport(
        Guid id,
        Guid portfolioId,
        Guid? strategyId,
        EvaluationPeriod period,
        DateTime startDate,
        DateTime endDate,
        ReturnMetrics returnMetrics,
        RiskMetrics riskMetrics,
        TradeMetrics tradeMetrics,
        EvaluationScore score) : base(id)
    {
        PortfolioId = portfolioId;
        StrategyId = strategyId;
        Period = period;
        StartDate = startDate;
        EndDate = endDate;
        ReturnMetrics = returnMetrics;
        RiskMetrics = riskMetrics;
        TradeMetrics = tradeMetrics;
        Score = score;
        GeneratedAt = DateTime.UtcNow;
    }

    public void AddSnapshot(PerformanceSnapshot snapshot)
    {
        _snapshots.Add(snapshot);
    }

    public void AddBenchmark(BenchmarkComparison benchmark)
    {
        _benchmarks.Add(benchmark);
    }

    public void AddEvaluation(StrategyEvaluation evaluation)
    {
        _evaluations.Add(evaluation);
    }

    public void AddFeedback(FeedbackEntry feedback)
    {
        _feedback.Add(feedback);
    }

    public void UpdateMetrics(
        ReturnMetrics returnMetrics,
        RiskMetrics riskMetrics,
        TradeMetrics tradeMetrics,
        EvaluationScore score)
    {
        ReturnMetrics = returnMetrics;
        RiskMetrics = riskMetrics;
        TradeMetrics = tradeMetrics;
        Score = score;
        GeneratedAt = DateTime.UtcNow;
    }
}
