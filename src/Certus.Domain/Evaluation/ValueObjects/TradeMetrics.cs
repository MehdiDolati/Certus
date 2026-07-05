using Certus.Domain.SharedKernel;

namespace Certus.Domain.Evaluation.ValueObjects;

public record TradeMetrics 
{
    public int TotalTrades { get; }
    public decimal WinRate { get; }
    public decimal ProfitFactor { get; }
    public decimal AvgWin { get; }
    public decimal AvgLoss { get; }
    public decimal Expectancy { get; }

    public TradeMetrics(
        int totalTrades,
        decimal winRate,
        decimal profitFactor,
        decimal avgWin,
        decimal avgLoss,
        decimal expectancy)
    {
        TotalTrades = totalTrades;
        WinRate = winRate;
        ProfitFactor = profitFactor;
        AvgWin = avgWin;
        AvgLoss = avgLoss;
        Expectancy = expectancy;
    }

    public bool IsProfitable => Expectancy > 0;
    public bool IsConsistent => WinRate > 0.5m && ProfitFactor > 1.5m;
}
