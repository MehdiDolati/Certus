namespace Certus.Application.Platform.DTOs;

public record PlatformPortfolioDto
{
    public string ExternalId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public decimal Balance { get; init; }
    public decimal Equity { get; init; }
    public decimal Margin { get; init; }
    public decimal FreeMargin { get; init; }
    public decimal Profit { get; init; }
    public DateTime Timestamp { get; init; }
    public List<PlatformStrategySummaryDto> Strategies { get; init; } = [];
    public Guid? CertusPortfolioId { get; init; }
}

public record PlatformStrategySummaryDto
{
    public string ExternalId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public decimal Profit { get; init; }
    public int TotalTrades { get; init; }
    public DateTime? LastTradeTime { get; init; }
}
