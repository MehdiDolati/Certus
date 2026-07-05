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
}
