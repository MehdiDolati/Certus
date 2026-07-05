using Certus.Domain.RiskAndPortfolio.Enums;
using Certus.Domain.SharedKernel;

namespace Certus.Domain.RiskAndPortfolio.Entities;

public class RiskAssessment : Entity
{
    public Guid PortfolioId { get; private set; }
    public RiskLevel OverallLevel { get; private set; }
    public decimal PortfolioVaR { get; private set; }
    public decimal CurrentDrawdown { get; private set; }
    public decimal MaxDrawdown { get; private set; }
    public decimal SharpeRatio { get; private set; }
    public decimal Volatility { get; private set; }
    public DateTime AssessedAt { get; private set; }
    public string Notes { get; private set; } = string.Empty;

    private RiskAssessment() { }

    public RiskAssessment(
        Guid id,
        Guid portfolioId,
        RiskLevel overallLevel,
        decimal portfolioVaR,
        decimal currentDrawdown,
        decimal maxDrawdown,
        decimal sharpeRatio,
        decimal volatility,
        string notes = "") : base(id)
    {
        PortfolioId = portfolioId;
        OverallLevel = overallLevel;
        PortfolioVaR = portfolioVaR;
        CurrentDrawdown = currentDrawdown;
        MaxDrawdown = maxDrawdown;
        SharpeRatio = sharpeRatio;
        Volatility = volatility;
        Notes = notes;
        AssessedAt = DateTime.UtcNow;
    }
}
