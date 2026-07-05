using Certus.Domain.SharedKernel;

namespace Certus.Domain.Evaluation.Entities;

public class PerformanceSnapshot : Entity
{
    public Guid ReportId { get; private set; }
    public Guid PortfolioId { get; private set; }
    public Guid? StrategyId { get; private set; }
    public DateOnly Date { get; private set; }
    public decimal ActualReturn { get; private set; }
    public decimal SupposedReturn { get; private set; }
    public decimal DailyPnL { get; private set; }
    public decimal Equity { get; private set; }
    public decimal Drawdown { get; private set; }
    public decimal Sharpe { get; private set; }

    private PerformanceSnapshot() { }

    public PerformanceSnapshot(
        Guid id,
        Guid reportId,
        Guid portfolioId,
        Guid? strategyId,
        DateOnly date,
        decimal actualReturn,
        decimal supposedReturn,
        decimal dailyPnL,
        decimal equity,
        decimal drawdown,
        decimal sharpe) : base(id)
    {
        ReportId = reportId;
        PortfolioId = portfolioId;
        StrategyId = strategyId;
        Date = date;
        ActualReturn = actualReturn;
        SupposedReturn = supposedReturn;
        DailyPnL = dailyPnL;
        Equity = equity;
        Drawdown = drawdown;
        Sharpe = sharpe;
    }
}
