using Certus.Domain.SharedKernel;

namespace Certus.Domain.Strategy.ValueObjects;

public record BacktestMetrics 
{
    public decimal TotalReturn { get; }
    public decimal SharpeRatio { get; }
    public decimal MaxDrawdown { get; }
    public decimal WinRate { get; }
    public decimal ProfitFactor { get; }
    public decimal CalmarRatio { get; }
    public decimal SortinoRatio { get; }
    public int TotalTrades { get; }
    public decimal AvgTradeDuration { get; }

    public BacktestMetrics(
        decimal totalReturn,
        decimal sharpeRatio,
        decimal maxDrawdown,
        decimal winRate,
        decimal profitFactor,
        decimal calmarRatio,
        decimal sortinoRatio,
        int totalTrades,
        decimal avgTradeDuration)
    {
        TotalReturn = totalReturn;
        SharpeRatio = sharpeRatio;
        MaxDrawdown = maxDrawdown;
        WinRate = winRate;
        ProfitFactor = profitFactor;
        CalmarRatio = calmarRatio;
        SortinoRatio = sortinoRatio;
        TotalTrades = totalTrades;
        AvgTradeDuration = avgTradeDuration;
    }

    public bool IsProfitable => TotalReturn > 0;
    public bool IsHighQuality => SharpeRatio > 1m && MaxDrawdown < 0.2m && WinRate > 0.5m;
}
