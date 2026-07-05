using Certus.Domain.Platform.ValueObjects;

namespace Certus.Domain.Platform.Interfaces;

public interface IPlatformAdapter
{
    string PlatformId { get; }
    string PlatformName { get; }
    Task<PlatformConnectionResult> ConnectAsync(PlatformConfig config);
    Task DisconnectAsync();
    Task<PlatformStatus> GetStatusAsync();
    Task<IReadOnlyList<PlatformPortfolio>> GetPortfoliosAsync();
    Task<PlatformPortfolio?> GetPortfolioAsync(string externalId);
    Task<IReadOnlyList<PlatformStrategy>> GetStrategiesAsync(string portfolioExternalId);
    Task<IReadOnlyList<PlatformTrade>> GetTradesAsync(string strategyExternalId, DateTime? from = null, DateTime? to = null);
}

public record PlatformConnectionResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public DateTime ConnectedAt { get; init; }
}

public record PlatformStatus
{
    public bool IsConnected { get; init; }
    public DateTime? LastDataReceived { get; init; }
    public int PortfolioCount { get; init; }
    public int StrategyCount { get; init; }
    public int TradeCount { get; init; }
    public string? ErrorMessage { get; init; }
}
