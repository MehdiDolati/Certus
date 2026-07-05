using Certus.Domain.SharedKernel;

namespace Certus.Domain.Execution.ValueObjects;

public record TradeLifecycle 
{
    public DateTime SignalTime { get; }
    public DateTime? OrderTime { get; }
    public DateTime? FillTime { get; }
    public DateTime? CloseTime { get; }

    public TradeLifecycle(
        DateTime signalTime,
        DateTime? orderTime,
        DateTime? fillTime,
        DateTime? closeTime)
    {
        SignalTime = signalTime;
        OrderTime = orderTime;
        FillTime = fillTime;
        CloseTime = closeTime;
    }

    public TimeSpan? SignalToOrder => OrderTime.HasValue ? OrderTime.Value - SignalTime : null;
    public TimeSpan? OrderToFill => FillTime.HasValue && OrderTime.HasValue ? FillTime.Value - OrderTime.Value : null;
    public TimeSpan? TotalDuration => CloseTime.HasValue ? CloseTime.Value - SignalTime : null;
}
