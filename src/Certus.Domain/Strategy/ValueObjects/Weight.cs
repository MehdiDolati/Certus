using Certus.Domain.SharedKernel;

namespace Certus.Domain.Strategy.ValueObjects;

public record Weight 
{
    public decimal Value { get; }

    public Weight(decimal value)
    {
        if (value < 0m || value > 1m)
            throw new ArgumentOutOfRangeException(nameof(value), "Weight must be between 0.0 and 1.0");
        Value = value;
    }

    public static Weight Zero => new(0m);
    public static Weight Full => new(1m);

    public override string ToString() => $"{Value:P0}";
}
