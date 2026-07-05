namespace Certus.Application.Platform.DTOs;

public record PlatformDashboardDto
{
    public int TotalConnections { get; init; }
    public int ActiveConnections { get; init; }
    public int TotalPortfolios { get; init; }
    public int TotalStrategies { get; init; }
    public int TotalTrades { get; init; }
    public decimal TotalPnL { get; init; }
    public List<PlatformConnectionDto> Connections { get; init; } = [];
}
