using Certus.Domain.SharedKernel;

namespace Certus.Domain.Strategy.Entities;

public class BacktestTrade : Entity
{
    public Guid BacktestRunId { get; private set; }
    public string Symbol { get; private set; } = string.Empty;
    public string Side { get; private set; } = string.Empty;
    public DateTime EntryTime { get; private set; }
    public DateTime? ExitTime { get; private set; }
    public decimal EntryPrice { get; private set; }
    public decimal? ExitPrice { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal PnL { get; private set; }

    private BacktestTrade() { }

    public BacktestTrade(
        Guid id,
        Guid backtestRunId,
        string symbol,
        string side,
        DateTime entryTime,
        decimal entryPrice,
        decimal quantity) : base(id)
    {
        BacktestRunId = backtestRunId;
        Symbol = symbol;
        Side = side;
        EntryTime = entryTime;
        EntryPrice = entryPrice;
        Quantity = quantity;
    }

    public void Close(DateTime exitTime, decimal exitPrice)
    {
        ExitTime = exitTime;
        ExitPrice = exitPrice;
        PnL = Side == "Long"
            ? (exitPrice - EntryPrice) * Quantity
            : (EntryPrice - exitPrice) * Quantity;
    }
}
