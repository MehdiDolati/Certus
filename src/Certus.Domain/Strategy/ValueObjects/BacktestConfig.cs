using Certus.Domain.Market.ValueObjects;
using Certus.Domain.SharedKernel;

namespace Certus.Domain.Strategy.ValueObjects;

public record BacktestConfig 
{
    public DateTime StartDate { get; }
    public DateTime EndDate { get; }
    public Money InitialCapital { get; }
    public IReadOnlyList<Symbol> Symbols { get; }
    public TimeFrame TimeFrame { get; }

    public BacktestConfig(
        DateTime startDate,
        DateTime endDate,
        Money initialCapital,
        IReadOnlyList<Symbol> symbols,
        TimeFrame timeFrame)
    {
        if (endDate <= startDate)
            throw new ArgumentException("End date must be after start date");

        StartDate = startDate;
        EndDate = endDate;
        InitialCapital = initialCapital;
        Symbols = symbols;
        TimeFrame = timeFrame;
    }

    public TimeSpan Duration => EndDate - StartDate;
}
