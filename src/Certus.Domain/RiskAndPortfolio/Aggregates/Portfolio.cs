using Certus.Domain.RiskAndPortfolio.Enums;
using Certus.Domain.RiskAndPortfolio.Entities;
using Certus.Domain.RiskAndPortfolio.ValueObjects;
using Certus.Domain.SharedKernel;

namespace Certus.Domain.RiskAndPortfolio.Aggregates;

public class Portfolio : AggregateRoot
{
    private readonly List<RiskLimit> _riskLimits = [];
    private readonly List<AllocationRule> _allocationRules = [];
    private readonly List<CapitalAllocation> _allocations = [];

    public IReadOnlyList<RiskLimit> RiskLimits => _riskLimits.AsReadOnly();
    public IReadOnlyList<AllocationRule> AllocationRules => _allocationRules.AsReadOnly();
    public IReadOnlyList<CapitalAllocation> Allocations => _allocations.AsReadOnly();

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public PortfolioStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public decimal TargetReturn { get; private set; }
    public decimal TargetSharpe { get; private set; }
    public Money AllocatedCapital { get; private set; }
    public AdaptiveRiskParams? AdaptiveRiskParams { get; private set; }
    public MarketRegime CurrentMarketRegime { get; private set; }

    public bool IsActive => Status == PortfolioStatus.Active;
    public bool CanAddStrategy() => Status == PortfolioStatus.Active;
    public decimal AllocatedCapitalInMillions => AllocatedCapital.Amount / 1_000_000m;

    private Portfolio() { }

    public Portfolio(
        Guid id,
        string name,
        decimal targetReturn,
        decimal targetSharpe,
        Money allocatedCapital,
        string description = "") : base(id)
    {
        Name = name;
        Description = description;
        TargetReturn = targetReturn;
        TargetSharpe = targetSharpe;
        AllocatedCapital = allocatedCapital;
        Status = PortfolioStatus.Active;
        CurrentMarketRegime = MarketRegime.Normal;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddRiskLimit(RiskLimit riskLimit)
    {
        _riskLimits.Add(riskLimit);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveRiskLimit(Guid riskLimitId)
    {
        var limit = _riskLimits.FirstOrDefault(l => l.Id == riskLimitId);
        if (limit is not null)
        {
            _riskLimits.Remove(limit);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void AddAllocationRule(AllocationRule rule)
    {
        _allocationRules.Add(rule);
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddCapitalAllocation(CapitalAllocation allocation)
    {
        _allocations.Add(allocation);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string name, string description, decimal targetReturn, decimal targetSharpe)
    {
        Name = name;
        Description = description;
        TargetReturn = targetReturn;
        TargetSharpe = targetSharpe;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateCapital(Money newCapital)
    {
        AllocatedCapital = newCapital;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new Events.CapitalReallocated
        {
            PortfolioId = Id,
            NewCapital = newCapital
        });
    }

    public void SetAdaptiveRiskParams(AdaptiveRiskParams adaptiveParams)
    {
        AdaptiveRiskParams = adaptiveParams;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateMarketRegime(MarketRegime regime)
    {
        CurrentMarketRegime = regime;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Pause()
    {
        if (Status != PortfolioStatus.Active)
            throw new InvalidOperationException("Can only pause an active portfolio");

        Status = PortfolioStatus.Paused;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new Events.PortfolioStatusChanged
        {
            PortfolioId = Id,
            OldStatus = PortfolioStatus.Active,
            NewStatus = PortfolioStatus.Paused
        });
    }

    public void Resume()
    {
        if (Status != PortfolioStatus.Paused)
            throw new InvalidOperationException("Can only resume a paused portfolio");

        Status = PortfolioStatus.Active;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new Events.PortfolioStatusChanged
        {
            PortfolioId = Id,
            OldStatus = PortfolioStatus.Paused,
            NewStatus = PortfolioStatus.Active
        });
    }

    public void Close()
    {
        if (Status == PortfolioStatus.Closed)
            throw new InvalidOperationException("Portfolio is already closed");

        var oldStatus = Status;
        Status = PortfolioStatus.Closed;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new Events.PortfolioStatusChanged
        {
            PortfolioId = Id,
            OldStatus = oldStatus,
            NewStatus = PortfolioStatus.Closed
        });
    }
}
