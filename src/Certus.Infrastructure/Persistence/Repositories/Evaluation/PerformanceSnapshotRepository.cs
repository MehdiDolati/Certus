using Certus.Domain.Evaluation.Entities;
using Certus.Domain.Evaluation.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Certus.Infrastructure.Persistence.Repositories.Evaluation;

public class PerformanceSnapshotRepository : IPerformanceSnapshotRepository
{
    private readonly CertusDbContext _db;

    public PerformanceSnapshotRepository(CertusDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<PerformanceSnapshot>> GetByPortfolioIdAsync(Guid portfolioId, DateTime? from, DateTime? to)
    {
        var query = _db.PerformanceSnapshots.AsNoTracking()
            .Where(p => p.PortfolioId == portfolioId);

        if (from.HasValue)
            query = query.Where(p => p.Date >= DateOnly.FromDateTime(from.Value));

        if (to.HasValue)
            query = query.Where(p => p.Date <= DateOnly.FromDateTime(to.Value));

        return await query.ToListAsync();
    }

    public async Task<IReadOnlyList<PerformanceSnapshot>> GetByStrategyIdAsync(Guid strategyId, DateTime? from, DateTime? to)
    {
        var query = _db.PerformanceSnapshots.AsNoTracking()
            .Where(p => p.StrategyId == strategyId);

        if (from.HasValue)
            query = query.Where(p => p.Date >= DateOnly.FromDateTime(from.Value));

        if (to.HasValue)
            query = query.Where(p => p.Date <= DateOnly.FromDateTime(to.Value));

        return await query.ToListAsync();
    }

    public async Task AddAsync(PerformanceSnapshot snapshot)
    {
        await _db.PerformanceSnapshots.AddAsync(snapshot);
    }

    public async Task AddRangeAsync(IEnumerable<PerformanceSnapshot> snapshots)
    {
        await _db.PerformanceSnapshots.AddRangeAsync(snapshots);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _db.SaveChangesAsync();
    }
}
