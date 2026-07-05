using Certus.Domain.Execution.Enums;
using Certus.Domain.SharedKernel;

namespace Certus.Domain.Execution.ValueObjects;

public record ExecutionResult 
{
    public Guid OrderId { get; }
    public OrderStatus Status { get; }
    public decimal FilledQuantity { get; }
    public decimal AverageFillPrice { get; }
    public decimal Fees { get; }
    public decimal Slippage { get; }
    public string? ErrorMessage { get; }

    public ExecutionResult(
        Guid orderId,
        OrderStatus status,
        decimal filledQuantity,
        decimal averageFillPrice,
        decimal fees,
        decimal slippage,
        string? errorMessage = null)
    {
        OrderId = orderId;
        Status = status;
        FilledQuantity = filledQuantity;
        AverageFillPrice = averageFillPrice;
        Fees = fees;
        Slippage = slippage;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccessful => Status == OrderStatus.Filled || Status == OrderStatus.PartiallyFilled;
}
