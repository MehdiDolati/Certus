using Certus.Domain.SharedKernel;

namespace Certus.Domain.Market.ValueObjects;

public record SignalStrength 
{
    public decimal Value { get; }
    public decimal Confidence { get; }

    public SignalStrength(decimal value, decimal confidence)
    {
        if (value < 0 || value > 1)
            throw new ArgumentOutOfRangeException(nameof(value), "Signal strength must be between 0 and 1");
        if (confidence < 0 || confidence > 1)
            throw new ArgumentOutOfRangeException(nameof(confidence), "Confidence must be between 0 and 1");

        Value = value;
        Confidence = confidence;
    }

    public bool IsStrong => Value >= 0.7m && Confidence >= 0.6m;
    public bool IsWeak => Value < 0.3m || Confidence < 0.4m;
}
