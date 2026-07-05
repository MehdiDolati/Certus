using Certus.Domain.Strategy.Enums;
using Certus.Domain.Strategy.ValueObjects;
using Certus.Domain.SharedKernel;

namespace Certus.Domain.Strategy.Entities;

public class BacktestRun : Entity
{
    private readonly List<BacktestTrade> _trades = [];
    public IReadOnlyList<BacktestTrade> Trades => _trades.AsReadOnly();

    public Guid StrategyId { get; private set; }
    public BacktestConfig Config { get; private set; }
    public BacktestStatus Status { get; private set; }
    public BacktestMetrics? Metrics { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? ErrorMessage { get; private set; }

    private BacktestRun() { }

    public BacktestRun(Guid id, Guid strategyId, BacktestConfig config) : base(id)
    {
        StrategyId = strategyId;
        Config = config;
        Status = BacktestStatus.Pending;
        StartedAt = DateTime.UtcNow;
    }

    public void Start()
    {
        Status = BacktestStatus.Running;
        StartedAt = DateTime.UtcNow;
    }

    public void Complete(BacktestMetrics metrics)
    {
        Status = BacktestStatus.Completed;
        Metrics = metrics;
        CompletedAt = DateTime.UtcNow;
    }

    public void Fail(string errorMessage)
    {
        Status = BacktestStatus.Failed;
        ErrorMessage = errorMessage;
        CompletedAt = DateTime.UtcNow;
    }

    public void AddTrade(BacktestTrade trade)
    {
        _trades.Add(trade);
    }
}
