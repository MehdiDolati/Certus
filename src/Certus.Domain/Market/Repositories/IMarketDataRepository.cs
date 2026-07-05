using Certus.Domain.Market.Aggregates;
using Certus.Domain.Market.ValueObjects;
using Certus.Domain.SharedKernel;

namespace Certus.Domain.Market.Repositories;

public interface IMarketDataRepository
{
    Task<MarketDataSeries?> GetBySymbolAsync(Symbol symbol, TimeFrame timeframe);
    Task<IReadOnlyList<PriceBar>> GetBarsAsync(Symbol symbol, TimeFrame timeframe, DateTime from, DateTime to);
    Task AddOrUpdateAsync(MarketDataSeries series);
    Task<int> SaveChangesAsync();
}
