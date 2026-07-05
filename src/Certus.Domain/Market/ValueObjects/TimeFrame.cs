namespace Certus.Domain.Market.ValueObjects;

public record TimeFrame(string Value)
{
    public static TimeFrame OneMinute => new("1m");
    public static TimeFrame FiveMinutes => new("5m");
    public static TimeFrame FifteenMinutes => new("15m");
    public static TimeFrame OneHour => new("1h");
    public static TimeFrame FourHours => new("4h");
    public static TimeFrame OneDay => new("1d");
    public static TimeFrame OneWeek => new("1w");

    public override string ToString() => Value;
}
