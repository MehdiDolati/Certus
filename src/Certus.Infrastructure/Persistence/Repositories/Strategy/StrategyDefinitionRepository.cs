using Certus.Domain.Strategy.Aggregates;
using Certus.Domain.Strategy.Enums;
using Certus.Domain.Strategy.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Certus.Infrastructure.Persistence.Repositories.Strategy;

public class StrategyDefinitionRepository : IStrategyDefinitionRepository
{
    private readonly CertusDbContext _db;

    public StrategyDefinitionRepository(CertusDbContext db)
    {
        _db = db;
    }

    public async Task<StrategyDefinition?> GetByIdAsync(Guid id)
    {
        return await _db.Strategies.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IReadOnlyList<StrategyDefinition>> GetByPortfolioIdAsync(Guid portfolioId)
    {
        return await _db.Strategies.AsNoTracking()
            .Where(s => _db.StrategySlots.Any(slot => slot.StrategyId == s.Id && slot.PortfolioId == portfolioId))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<StrategyDefinition>> GetByStatusAsync(StrategyStatus status)
    {
        return await _db.Strategies.AsNoTracking()
            .Where(s => s.Status == status)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<StrategyDefinition>> GetAllAsync()
    {
        return await _db.Strategies.AsNoTracking().ToListAsync();
    }

    public async Task AddAsync(StrategyDefinition strategy)
    {
        await _db.Strategies.AddAsync(strategy);
    }

    public void Update(StrategyDefinition strategy)
    {
        _db.Strategies.Update(strategy);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _db.SaveChangesAsync();
    }
}
