namespace Certus.Application.Platform;

public class DeviationCalculator
{
    public DeviationResult Calculate(decimal actual, decimal expected)
    {
        decimal percentage = expected == 0
            ? (actual > 0 ? 100m : 0m)
            : (actual - expected) / Math.Abs(expected) * 100m;

        var direction = percentage > 0
            ? DeviationDirection.Upward
            : percentage < 0
                ? DeviationDirection.Downward
                : DeviationDirection.Neutral;

        return new DeviationResult
        {
            Percentage = Math.Round(percentage, 2),
            Direction = direction,
            Actual = actual,
            Expected = expected
        };
    }

    public TradeDeviationResult CalculateTradeDeviation(TradeOutcome actual, TradeOutcome expected)
    {
        var pnlDeviation = Calculate(actual.PnL, expected.PnL);
        var durationDeviation = Calculate(
            (decimal)actual.Duration.TotalMinutes,
            (decimal)expected.Duration.TotalMinutes);

        return new TradeDeviationResult
        {
            PnLDeviation = pnlDeviation,
            DurationDeviation = durationDeviation
        };
    }
}

public record DeviationResult
{
    public decimal Percentage { get; init; }
    public DeviationDirection Direction { get; init; }
    public decimal Actual { get; init; }
    public decimal Expected { get; init; }
}

public enum DeviationDirection
{
    Upward,
    Downward,
    Neutral
}

public record TradeOutcome
{
    public decimal PnL { get; init; }
    public TimeSpan Duration { get; init; }
}

public record TradeDeviationResult
{
    public DeviationResult PnLDeviation { get; init; } = new();
    public DeviationResult DurationDeviation { get; init; } = new();
}
