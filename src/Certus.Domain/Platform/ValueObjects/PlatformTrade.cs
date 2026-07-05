using Certus.Domain.SharedKernel;

namespace Certus.Domain.Platform.ValueObjects;

public record PlatformTrade
{
    public string ExternalId { get; init; } = string.Empty;
    public string StrategyExternalId { get; init; } = string.Empty;
    public string Symbol { get; init; } = string.Empty;
    public TradeSide Side { get; init; }
    public decimal Volume { get; init; }
    public decimal OpenPrice { get; init; }
    public decimal? ClosePrice { get; init; }
    public decimal StopLoss { get; init; }
    public decimal TakeProfit { get; init; }
    public decimal Profit { get; init; }
    public decimal Commission { get; init; }
    public decimal Swap { get; init; }
    public DateTime OpenTime { get; init; }
    public DateTime? CloseTime { get; init; }
    public string Comment { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}

public enum TradeSide
{
    Buy,
    Sell
}
