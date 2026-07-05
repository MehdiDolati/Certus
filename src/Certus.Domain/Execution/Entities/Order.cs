using Certus.Domain.Execution.Enums;
using Certus.Domain.SharedKernel;

namespace Certus.Domain.Execution.Entities;

public class Order : Entity
{
    public Guid TradeId { get; private set; }
    public string Symbol { get; private set; } = string.Empty;
    public TradeSide Side { get; private set; }
    public OrderType Type { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal? LimitPrice { get; private set; }
    public decimal? StopPrice { get; private set; }
    public decimal FilledQuantity { get; private set; }
    public decimal AverageFillPrice { get; private set; }
    public decimal TotalFees { get; private set; }
    public decimal Slippage { get; private set; }
    public ExecutionVenue Venue { get; private set; }
    public string? ExchangeOrderId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? FilledAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    public bool IsFullyFilled => FilledQuantity >= Quantity;
    public decimal FillPercentage => Quantity > 0 ? FilledQuantity / Quantity * 100m : 0m;

    private Order() { }

    public Order(
        Guid id,
        Guid tradeId,
        string symbol,
        TradeSide side,
        OrderType type,
        decimal quantity,
        ExecutionVenue venue,
        decimal? limitPrice = null,
        decimal? stopPrice = null) : base(id)
    {
        TradeId = tradeId;
        Symbol = symbol;
        Side = side;
        Type = type;
        Quantity = quantity;
        Venue = venue;
        LimitPrice = limitPrice;
        StopPrice = stopPrice;
        Status = OrderStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void Submit(string exchangeOrderId)
    {
        Status = OrderStatus.Submitted;
        ExchangeOrderId = exchangeOrderId;
    }

    public void Fill(decimal fillQuantity, decimal fillPrice, decimal fees)
    {
        FilledQuantity += fillQuantity;
        AverageFillPrice = FilledQuantity > 0
            ? ((AverageFillPrice * (FilledQuantity - fillQuantity)) + (fillPrice * fillQuantity)) / FilledQuantity
            : fillPrice;
        TotalFees += fees;
        Slippage = LimitPrice.HasValue ? Math.Abs(fillPrice - LimitPrice.Value) / LimitPrice.Value * 100m : 0m;

        Status = IsFullyFilled ? OrderStatus.Filled : OrderStatus.PartiallyFilled;
        if (IsFullyFilled) FilledAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = OrderStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
    }

    public void Reject(string reason)
    {
        Status = OrderStatus.Rejected;
        CancelledAt = DateTime.UtcNow;
    }
}
