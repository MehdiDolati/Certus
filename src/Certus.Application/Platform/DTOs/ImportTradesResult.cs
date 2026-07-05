namespace Certus.Application.Platform.DTOs;

public record ImportTradesResult
{
    public int TradesImported { get; init; }
    public int TradesSkipped { get; init; }
    public decimal TotalPnL { get; init; }
    public DateTime? EarliestTrade { get; init; }
    public DateTime? LatestTrade { get; init; }
    public List<string> Errors { get; init; } = [];
}
