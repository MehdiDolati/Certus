namespace Certus.Domain.Platform.ValueObjects;

public record PlatformPortfolio
{
    public string ExternalId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public decimal Balance { get; init; }
    public decimal Equity { get; init; }
    public decimal Margin { get; init; }
    public decimal FreeMargin { get; init; }
    public decimal Profit { get; init; }
    public DateTime Timestamp { get; init; }
    public IReadOnlyList<PlatformStrategy> Strategies { get; init; } = [];
}
