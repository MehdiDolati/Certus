using Certus.Domain.Market.Aggregates;
using Certus.Domain.Market.Repositories;
using Certus.Domain.Market.ValueObjects;
using Certus.Domain.SharedKernel;

namespace Certus.Infrastructure.Persistence.Repositories.Market;

public class MarketDataRepository : IMarketDataRepository
{
    private readonly CertusDbContext _db;

    public MarketDataRepository(CertusDbContext db)
    {
        _db = db;
    }

    public Task<MarketDataSeries?> GetBySymbolAsync(Symbol symbol, TimeFrame timeframe)
    {
        return Task.FromResult<MarketDataSeries?>(null);
    }

    public Task<IReadOnlyList<PriceBar>> GetBarsAsync(Symbol symbol, TimeFrame timeframe, DateTime from, DateTime to)
    {
        return Task.FromResult<IReadOnlyList<PriceBar>>(new List<PriceBar>());
    }

    public Task AddOrUpdateAsync(MarketDataSeries series)
    {
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync()
    {
        return Task.FromResult(0);
    }
}
