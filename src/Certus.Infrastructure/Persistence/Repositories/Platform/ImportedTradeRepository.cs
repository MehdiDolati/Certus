using Certus.Domain.Platform.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Certus.Infrastructure.Persistence.Repositories.Platform;

public class ImportedTradeRepository : IImportedTradeRepository
{
    private readonly CertusDbContext _db;

    public ImportedTradeRepository(CertusDbContext db)
    {
        _db = db;
    }

    public async Task<ImportedTrade?> GetByExternalIdAsync(string externalId)
    {
        return await _db.ImportedTrades.AsNoTracking()
            .FirstOrDefaultAsync(t => t.ExternalId == externalId);
    }

    public async Task<IReadOnlyList<ImportedTrade>> GetByStrategyIdAsync(Guid strategyId)
    {
        return await _db.ImportedTrades.AsNoTracking()
            .Where(t => t.StrategyId == strategyId)
            .OrderByDescending(t => t.OpenTime)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ImportedTrade>> GetByPortfolioIdAsync(Guid portfolioId)
    {
        return await _db.ImportedTrades.AsNoTracking()
            .Where(t => t.PortfolioId == portfolioId)
            .OrderByDescending(t => t.OpenTime)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ImportedTrade>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        return await _db.ImportedTrades.AsNoTracking()
            .Where(t => t.OpenTime >= from && t.OpenTime <= to)
            .OrderByDescending(t => t.OpenTime)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ImportedTrade>> GetOpenTradesAsync(Guid? strategyId = null)
    {
        var query = _db.ImportedTrades.AsNoTracking()
            .Where(t => t.CloseTime == null);

        if (strategyId.HasValue)
            query = query.Where(t => t.StrategyId == strategyId.Value);

        return await query.OrderByDescending(t => t.OpenTime).ToListAsync();
    }

    public async Task AddAsync(ImportedTrade trade)
    {
        await _db.ImportedTrades.AddAsync(trade);
    }

    public void Update(ImportedTrade trade)
    {
        _db.ImportedTrades.Update(trade);
    }

    public async Task<int> GetCountByStrategyAsync(Guid strategyId)
    {
        return await _db.ImportedTrades.AsNoTracking()
            .CountAsync(t => t.StrategyId == strategyId);
    }

    public async Task<decimal> GetTotalPnLByStrategyAsync(Guid strategyId)
    {
        return await _db.ImportedTrades.AsNoTracking()
            .Where(t => t.StrategyId == strategyId && t.CloseTime != null)
            .SumAsync(t => t.Profit + t.Commission + t.Swap);
    }
}
