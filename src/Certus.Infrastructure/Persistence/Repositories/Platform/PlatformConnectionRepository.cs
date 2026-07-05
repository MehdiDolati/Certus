using Certus.Domain.Platform.Aggregates;
using Certus.Domain.Platform.Enums;
using Certus.Domain.Platform.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Certus.Infrastructure.Persistence.Repositories.Platform;

public class PlatformConnectionRepository : IPlatformConnectionRepository
{
    private readonly CertusDbContext _db;

    public PlatformConnectionRepository(CertusDbContext db)
    {
        _db = db;
    }

    public async Task<PlatformConnection?> GetByIdAsync(Guid id)
    {
        return await _db.PlatformConnections.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IReadOnlyList<PlatformConnection>> GetAllAsync()
    {
        return await _db.PlatformConnections.AsNoTracking().ToListAsync();
    }

    public async Task<IReadOnlyList<PlatformConnection>> GetByPlatformAsync(string platformId)
    {
        return await _db.PlatformConnections.AsNoTracking()
            .Where(p => p.PlatformId == platformId)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<PlatformConnection>> GetByStatusAsync(PlatformConnectionStatus status)
    {
        return await _db.PlatformConnections.AsNoTracking()
            .Where(p => p.Status == status)
            .ToListAsync();
    }

    public async Task AddAsync(PlatformConnection connection)
    {
        await _db.PlatformConnections.AddAsync(connection);
    }

    public void Update(PlatformConnection connection)
    {
        _db.PlatformConnections.Update(connection);
    }
}
