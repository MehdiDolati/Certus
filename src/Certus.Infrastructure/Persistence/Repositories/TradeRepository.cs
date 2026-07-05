using Certus.Domain.Execution.Aggregates;
using Certus.Domain.Execution.Enums;
using Certus.Domain.Execution.Repositories;
using Certus.Domain.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Certus.Infrastructure.Persistence.Repositories;

public class TradeRepository : ITradeRepository
{
    private readonly CertusDbContext _db;

    public TradeRepository(CertusDbContext db)
    {
        _db = db;
    }

    public async Task<Trade?> GetByIdAsync(Guid id)
    {
        return await _db.Trades.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IReadOnlyList<Trade>> GetByPortfolioIdAsync(Guid portfolioId)
    {
        return await _db.Trades.AsNoTracking()
            .Where(t => t.PortfolioId == portfolioId)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Trade>> GetByStrategyIdAsync(Guid strategyId)
    {
        return await _db.Trades.AsNoTracking()
            .Where(t => t.StrategyId == strategyId)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Trade>> GetOpenTradesAsync(Guid? portfolioId, Guid? strategyId)
    {
        var query = _db.Trades.AsNoTracking()
            .Where(t => t.Status == TradeStatus.Open);

        if (portfolioId.HasValue)
            query = query.Where(t => t.PortfolioId == portfolioId.Value);

        if (strategyId.HasValue)
            query = query.Where(t => t.StrategyId == strategyId.Value);

        return await query.ToListAsync();
    }

    public async Task<IReadOnlyList<Trade>> GetFilteredAsync(
        Guid? portfolioId, Guid? strategyId, Symbol? symbol,
        TradeSide? side, TradeStatus? status,
        DateTime? dateFrom, DateTime? dateTo, string? search,
        int page, int pageSize)
    {
        var query = _db.Trades.AsNoTracking().AsQueryable();

        if (portfolioId.HasValue)
            query = query.Where(t => t.PortfolioId == portfolioId.Value);

        if (strategyId.HasValue)
            query = query.Where(t => t.StrategyId == strategyId.Value);

        if (symbol != null)
            query = query.Where(t => t.Symbol.Value == symbol.Value);

        if (side.HasValue)
            query = query.Where(t => t.Side == side.Value);

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        if (dateFrom.HasValue)
            query = query.Where(t => t.EntryTime >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(t => t.EntryTime <= dateTo.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(t => t.AgentReason.Contains(search));

        return await query
            .OrderByDescending(t => t.EntryTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountFilteredAsync(
        Guid? portfolioId, Guid? strategyId, Symbol? symbol,
        TradeSide? side, TradeStatus? status,
        DateTime? dateFrom, DateTime? dateTo, string? search)
    {
        var query = _db.Trades.AsNoTracking().AsQueryable();

        if (portfolioId.HasValue)
            query = query.Where(t => t.PortfolioId == portfolioId.Value);

        if (strategyId.HasValue)
            query = query.Where(t => t.StrategyId == strategyId.Value);

        if (symbol != null)
            query = query.Where(t => t.Symbol.Value == symbol.Value);

        if (side.HasValue)
            query = query.Where(t => t.Side == side.Value);

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        if (dateFrom.HasValue)
            query = query.Where(t => t.EntryTime >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(t => t.EntryTime <= dateTo.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(t => t.AgentReason.Contains(search));

        return await query.CountAsync();
    }

    public async Task AddAsync(Trade trade)
    {
        await _db.Trades.AddAsync(trade);
    }

    public void Update(Trade trade)
    {
        _db.Trades.Update(trade);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _db.SaveChangesAsync();
    }
}
