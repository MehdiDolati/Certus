using Certus.Domain.Execution.Enums;
using Certus.Domain.SharedKernel;

namespace Certus.Domain.Execution.ValueObjects;

public record OrderRequest 
{
    public Symbol Symbol { get; }
    public TradeSide Side { get; }
    public OrderType Type { get; }
    public decimal Quantity { get; }
    public decimal? Price { get; }
    public ExecutionVenue Venue { get; }

    public OrderRequest(
        Symbol symbol,
        TradeSide side,
        OrderType type,
        decimal quantity,
        ExecutionVenue venue,
        decimal? price = null)
    {
        Symbol = symbol;
        Side = side;
        Type = type;
        Quantity = quantity;
        Venue = venue;
        Price = price;
    }
}
