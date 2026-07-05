using Certus.Domain.SharedKernel;

namespace Certus.Domain.Evaluation.ValueObjects;

public record RiskMetrics 
{
    public decimal SharpeRatio { get; }
    public decimal SortinoRatio { get; }
    public decimal MaxDrawdown { get; }
    public decimal CalmarRatio { get; }
    public decimal Volatility { get; }
    public decimal VaR { get; }

    public RiskMetrics(
        decimal sharpeRatio,
        decimal sortinoRatio,
        decimal maxDrawdown,
        decimal calmarRatio,
        decimal volatility,
        decimal VaR)
    {
        SharpeRatio = sharpeRatio;
        SortinoRatio = sortinoRatio;
        MaxDrawdown = maxDrawdown;
        CalmarRatio = calmarRatio;
        Volatility = volatility;
        this.VaR = VaR;
    }

    public bool IsHighQuality => SharpeRatio > 1m && MaxDrawdown < 0.2m;
}
