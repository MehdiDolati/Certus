using Certus.Domain.Execution.Enums;
using Certus.Domain.Execution.Entities;
using Certus.Domain.Execution.ValueObjects;
using Certus.Domain.SharedKernel;

namespace Certus.Domain.Execution.Aggregates;

public class Trade : AggregateRoot
{
    private readonly List<Order> _orders = [];
    private readonly List<ExecutionLog> _executionLogs = [];

    public IReadOnlyList<Order> Orders => _orders.AsReadOnly();
    public IReadOnlyList<ExecutionLog> ExecutionLogs => _executionLogs.AsReadOnly();

    public Guid StrategyId { get; private set; }
    public Guid PortfolioId { get; private set; }
    public Symbol Symbol { get; private set; }
    public TradeSide Side { get; private set; }
    public DateTime EntryTime { get; private set; }
    public DateTime? ExitTime { get; private set; }
    public decimal EntryPrice { get; private set; }
    public decimal? ExitPrice { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal PnL { get; private set; }
    public decimal PnLPercent { get; private set; }
    public decimal Fees { get; private set; }
    public decimal Slippage { get; private set; }
    public TradeStatus Status { get; private set; }
    public string AgentReason { get; private set; } = string.Empty;
    public TradeLifecycle Lifecycle { get; private set; }

    public bool IsOpen => Status == TradeStatus.Open;
    public bool IsClosed => Status == TradeStatus.Closed;
    public TimeSpan? Duration => ExitTime.HasValue ? ExitTime.Value - EntryTime : null;
    public bool IsWinning => PnL > 0;

    private Trade() { }

    public Trade(
        Guid id,
        Guid strategyId,
        Guid portfolioId,
        Symbol symbol,
        TradeSide side,
        decimal entryPrice,
        decimal quantity,
        string agentReason = "") : base(id)
    {
        StrategyId = strategyId;
        PortfolioId = portfolioId;
        Symbol = symbol;
        Side = side;
        EntryPrice = entryPrice;
        Quantity = quantity;
        AgentReason = agentReason;
        Status = TradeStatus.Pending;
        EntryTime = DateTime.UtcNow;
        Lifecycle = new TradeLifecycle(DateTime.UtcNow, null, null, null);

        RaiseDomainEvent(new Events.TradeOpened
        {
            TradeId = id,
            PortfolioId = portfolioId,
            StrategyId = strategyId,
            Symbol = symbol,
            Side = side,
            Size = quantity
        });
    }

    public void Open(Order entryOrder)
    {
        if (Status != TradeStatus.Pending)
            throw new InvalidOperationException("Trade must be pending to open");

        Status = TradeStatus.Open;
        EntryPrice = entryOrder.AverageFillPrice;
        Fees += entryOrder.TotalFees;
        Slippage += entryOrder.Slippage;
        Lifecycle = new TradeLifecycle(Lifecycle.SignalTime, entryOrder.CreatedAt, null, null);

        _orders.Add(entryOrder);
        AddLog("Trade opened", $"Entry at {EntryPrice}");
    }

    public void Close(Order exitOrder)
    {
        if (Status != TradeStatus.Open)
            throw new InvalidOperationException("Trade must be open to close");

        Status = TradeStatus.Closed;
        ExitTime = DateTime.UtcNow;
        ExitPrice = exitOrder.AverageFillPrice;
        Fees += exitOrder.TotalFees;
        Slippage += exitOrder.Slippage;

        PnL = Side == TradeSide.Long
            ? (ExitPrice.Value - EntryPrice) * Quantity
            : (EntryPrice - ExitPrice.Value) * Quantity;

        PnLPercent = EntryPrice * Quantity > 0
            ? PnL / (EntryPrice * Quantity) * 100m
            : 0m;

        Lifecycle = new TradeLifecycle(Lifecycle.SignalTime, Lifecycle.OrderTime, ExitTime, null);

        _orders.Add(exitOrder);
        AddLog("Trade closed", $"Exit at {ExitPrice}, PnL: {PnL}");

        RaiseDomainEvent(new Events.TradeClosed
        {
            TradeId = Id,
            PnL = PnL,
            Duration = Duration ?? TimeSpan.Zero
        });
    }

    public void Cancel(string reason)
    {
        if (Status == TradeStatus.Closed)
            throw new InvalidOperationException("Cannot cancel a closed trade");

        Status = TradeStatus.Cancelled;
        ExitTime = DateTime.UtcNow;
        Lifecycle = new TradeLifecycle(Lifecycle.SignalTime, Lifecycle.OrderTime, null, DateTime.UtcNow);

        AddLog("Trade cancelled", reason);

        RaiseDomainEvent(new Events.TradeCancelled
        {
            TradeId = Id,
            Reason = reason
        });
    }

    private void AddLog(string action, string details)
    {
        var log = new ExecutionLog(Guid.NewGuid(), Id, action, details);
        _executionLogs.Add(log);
    }
}
