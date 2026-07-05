using Certus.Domain.SharedKernel;

namespace Certus.Domain.Strategy.ValueObjects;

public record ConfidenceInterval 
{
    public decimal Lower { get; }
    public decimal Upper { get; }
    public decimal ConfidenceLevel { get; }

    public ConfidenceInterval(decimal lower, decimal upper, decimal confidenceLevel)
    {
        if (upper <= lower)
            throw new ArgumentException("Upper bound must be greater than lower bound");
        if (confidenceLevel <= 0 || confidenceLevel >= 1)
            throw new ArgumentOutOfRangeException(nameof(confidenceLevel));

        Lower = lower;
        Upper = upper;
        ConfidenceLevel = confidenceLevel;
    }

    public decimal Midpoint => (Lower + Upper) / 2m;
    public decimal Width => Upper - Lower;

    public bool Contains(decimal value) => value >= Lower && value <= Upper;
}
