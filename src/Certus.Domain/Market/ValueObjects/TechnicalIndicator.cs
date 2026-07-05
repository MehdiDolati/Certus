using Certus.Domain.Market.Enums;

namespace Certus.Domain.Market.ValueObjects;

public record TechnicalIndicator(
    IndicatorType Type,
    string Name,
    decimal Value,
    DateTime CalculatedAt,
    Dictionary<string, decimal>? Parameters = null)
{
    public bool IsOverbought => Type == IndicatorType.RSI && Value > 70m;
    public bool IsOversold => Type == IndicatorType.RSI && Value < 30m;
}
