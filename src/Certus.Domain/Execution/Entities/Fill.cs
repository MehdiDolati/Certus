using Certus.Domain.Execution.Enums;
using Certus.Domain.SharedKernel;

namespace Certus.Domain.Execution.Entities;

public class Fill : Entity
{
    public Guid OrderId { get; private set; }
    public string ExchangeFillId { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal Fees { get; private set; }
    public DateTime FilledAt { get; private set; }

    private Fill() { }

    public Fill(
        Guid id,
        Guid orderId,
        string exchangeFillId,
        decimal price,
        decimal quantity,
        decimal fees,
        DateTime filledAt) : base(id)
    {
        OrderId = orderId;
        ExchangeFillId = exchangeFillId;
        Price = price;
        Quantity = quantity;
        Fees = fees;
        FilledAt = filledAt;
    }
}
