using Certus.Domain.Platform.Repositories;
using Certus.Domain.Platform.ValueObjects;

namespace Certus.Infrastructure.Persistence.Repositories.Platform;

public class PlatformDataRepository : IPlatformDataRepository
{
    public Task<PlatformPortfolio?> GetLatestPortfolioSnapshotAsync(Guid connectionId, string externalPortfolioId)
    {
        return Task.FromResult<PlatformPortfolio?>(null);
    }

    public Task<IReadOnlyList<PlatformStrategy>> GetLatestStrategySnapshotsAsync(Guid connectionId, string externalPortfolioId)
    {
        return Task.FromResult<IReadOnlyList<PlatformStrategy>>([]);
    }

    public Task SavePortfolioSnapshotAsync(Guid connectionId, PlatformPortfolio portfolio)
    {
        return Task.CompletedTask;
    }

    public Task SaveTradeSnapshotAsync(Guid connectionId, PlatformTrade trade)
    {
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<PlatformTrade>> GetTradesByStrategyAsync(Guid connectionId, string strategyExternalId)
    {
        return Task.FromResult<IReadOnlyList<PlatformTrade>>([]);
    }

    public Task<DateTime?> GetLastSnapshotTimeAsync(Guid connectionId)
    {
        return Task.FromResult<DateTime?>(null);
    }
}
