using Certus.Domain.SharedKernel;

namespace Certus.Domain.Evaluation.ValueObjects;

public record ReturnMetrics
{
    public decimal TotalReturn { get; init; }
    public decimal AnnualizedReturn { get; init; }
    public IReadOnlyList<decimal> MonthlyReturns { get; init; } = [];
    public IReadOnlyList<decimal> DailyReturns { get; init; } = [];

    public ReturnMetrics() { }

    public ReturnMetrics(
        decimal totalReturn,
        decimal annualizedReturn,
        IReadOnlyList<decimal> monthlyReturns,
        IReadOnlyList<decimal> dailyReturns)
    {
        TotalReturn = totalReturn;
        AnnualizedReturn = annualizedReturn;
        MonthlyReturns = monthlyReturns;
        DailyReturns = dailyReturns;
    }

    public bool IsPositive => TotalReturn > 0;
    public decimal BestMonth => MonthlyReturns.Any() ? MonthlyReturns.Max() : 0m;
    public decimal WorstMonth => MonthlyReturns.Any() ? MonthlyReturns.Min() : 0m;
}
