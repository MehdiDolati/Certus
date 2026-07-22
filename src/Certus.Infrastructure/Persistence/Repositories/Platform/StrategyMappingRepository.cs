using Certus.Domain.Platform.Entities;
using Certus.Domain.Platform.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Certus.Infrastructure.Persistence.Repositories.Platform;

public class StrategyMappingRepository : IStrategyMappingRepository
{
    private readonly CertusDbContext _db;

    public StrategyMappingRepository(CertusDbContext db)
    {
        _db = db;
    }

    public async Task<StrategyMapping?> GetByConnectionAndExternalIdAsync(Guid connectionId, string strategyExternalId)
    {
        return await _db.StrategyMappings.AsNoTracking()
            .FirstOrDefaultAsync(m => m.ConnectionId == connectionId && m.StrategyExternalId == strategyExternalId);
    }

    public async Task<IReadOnlyList<StrategyMapping>> GetByConnectionIdAsync(Guid connectionId)
    {
        return await _db.StrategyMappings.AsNoTracking()
            .Where(m => m.ConnectionId == connectionId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(StrategyMapping mapping)
    {
        await _db.StrategyMappings.AddAsync(mapping);
    }
}
