using Certus.Domain.Platform.ValueObjects;

namespace Certus.Domain.Platform.Repositories;

public interface IPlatformDataRepository
{
    Task<PlatformPortfolio?> GetLatestPortfolioSnapshotAsync(Guid connectionId, string externalPortfolioId);
    Task<IReadOnlyList<PlatformStrategy>> GetLatestStrategySnapshotsAsync(Guid connectionId, string externalPortfolioId);
    Task SavePortfolioSnapshotAsync(Guid connectionId, PlatformPortfolio portfolio);
    Task SaveTradeSnapshotAsync(Guid connectionId, PlatformTrade trade);
    Task<IReadOnlyList<PlatformTrade>> GetTradesByStrategyAsync(Guid connectionId, string strategyExternalId);
    Task<DateTime?> GetLastSnapshotTimeAsync(Guid connectionId);
}
