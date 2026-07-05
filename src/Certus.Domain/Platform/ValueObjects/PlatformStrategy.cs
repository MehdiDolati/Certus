namespace Certus.Domain.Platform.ValueObjects;

public record PlatformStrategy
{
    public string ExternalId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public decimal Profit { get; init; }
    public int TotalTrades { get; init; }
    public int WinningTrades { get; init; }
    public int LosingTrades { get; init; }
    public DateTime? LastTradeTime { get; init; }
    public DateTime Timestamp { get; init; }
    public IReadOnlyList<OpenPosition> OpenPositions { get; init; } = [];
    public StrategyStats? Stats { get; init; }
}

public record OpenPosition
{
    public int Ticket { get; init; }
    public string Symbol { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public decimal Volume { get; init; }
    public decimal OpenPrice { get; init; }
    public decimal CurrentPrice { get; init; }
    public decimal StopLoss { get; init; }
    public decimal TakeProfit { get; init; }
    public decimal Profit { get; init; }
    public DateTime OpenTime { get; init; }
    public string Comment { get; init; } = string.Empty;
}

public record StrategyStats
{
    public int WinningTrades { get; init; }
    public int LosingTrades { get; init; }
    public decimal TotalProfit { get; init; }
    public decimal TotalLoss { get; init; }
    public decimal WinRate { get; init; }
    public decimal ProfitFactor { get; init; }
    public decimal MaxDrawdown { get; init; }
}
