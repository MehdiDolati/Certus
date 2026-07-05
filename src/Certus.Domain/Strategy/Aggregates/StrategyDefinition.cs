using Certus.Domain.Strategy.Enums;
using Certus.Domain.Strategy.Entities;
using Certus.Domain.Strategy.ValueObjects;
using Certus.Domain.SharedKernel;

namespace Certus.Domain.Strategy.Aggregates;

public class StrategyDefinition : AggregateRoot
{
    private readonly List<StrategyParameter> _parameters = [];
    private readonly List<BacktestRun> _backtestRuns = [];
    private readonly List<StrategySlot> _slots = [];

    public IReadOnlyList<StrategyParameter> Parameters => _parameters.AsReadOnly();
    public IReadOnlyList<BacktestRun> BacktestRuns => _backtestRuns.AsReadOnly();
    public IReadOnlyList<StrategySlot> Slots => _slots.AsReadOnly();

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public StrategyType Type { get; private set; }
    public StrategyStatus Status { get; private set; }
    public decimal TargetReturn { get; private set; }
    public RiskProfile? RiskProfile { get; private set; }
    public ConfidenceInterval? PredictedPerformance { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public bool IsActive => Status == StrategyStatus.Active;
    public bool CanBeActivated => Status == StrategyStatus.Draft || Status == StrategyStatus.Backtesting;

    private StrategyDefinition() { }

    public StrategyDefinition(
        Guid id,
        string name,
        StrategyType type,
        decimal targetReturn,
        string description = "") : base(id)
    {
        Name = name;
        Type = type;
        TargetReturn = targetReturn;
        Description = description;
        Status = StrategyStatus.Draft;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string name, StrategyType type, decimal targetReturn, string description)
    {
        Name = name;
        Type = type;
        TargetReturn = targetReturn;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddParameter(StrategyParameter parameter)
    {
        _parameters.Add(parameter);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveParameter(Guid parameterId)
    {
        var parameter = _parameters.FirstOrDefault(p => p.Id == parameterId);
        if (parameter is not null)
        {
            _parameters.Remove(parameter);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void AddBacktestRun(BacktestRun run)
    {
        _backtestRuns.Add(run);
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignToPortfolio(Guid portfolioId, Weight weight)
    {
        if (_slots.Any(s => s.PortfolioId == portfolioId))
            throw new InvalidOperationException("Strategy is already assigned to this portfolio");

        var slot = new StrategySlot(Guid.NewGuid(), portfolioId, weight);
        _slots.Add(slot);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveFromPortfolio(Guid portfolioId)
    {
        var slot = _slots.FirstOrDefault(s => s.PortfolioId == portfolioId);
        if (slot is not null)
        {
            _slots.Remove(slot);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void Activate()
    {
        if (!CanBeActivated)
            throw new InvalidOperationException($"Cannot activate strategy in {Status} status");

        Status = StrategyStatus.Active;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new Events.StrategyActivated
        {
            StrategyId = Id,
            Name = Name,
            Type = Type
        });
    }

    public void Pause()
    {
        if (Status != StrategyStatus.Active)
            throw new InvalidOperationException("Can only pause an active strategy");

        Status = StrategyStatus.Paused;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new Events.StrategyDeactivated
        {
            StrategyId = Id,
            Reason = "Paused"
        });
    }

    public void Retire()
    {
        if (Status == StrategyStatus.Retired)
            throw new InvalidOperationException("Strategy is already retired");

        Status = StrategyStatus.Retired;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new Events.StrategyDeactivated
        {
            StrategyId = Id,
            Reason = "Retired"
        });
    }

    public void SetRiskProfile(RiskProfile profile)
    {
        RiskProfile = profile;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetPredictedPerformance(ConfidenceInterval interval)
    {
        PredictedPerformance = interval;
        UpdatedAt = DateTime.UtcNow;
    }
}
