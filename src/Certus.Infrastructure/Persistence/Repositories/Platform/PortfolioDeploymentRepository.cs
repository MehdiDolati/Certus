using Certus.Domain.Platform.Aggregates;
using Certus.Domain.Platform.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Certus.Infrastructure.Persistence.Repositories.Platform;

public class PortfolioDeploymentRepository : IPortfolioDeploymentRepository
{
    private readonly CertusDbContext _db;

    public PortfolioDeploymentRepository(CertusDbContext db)
    {
        _db = db;
    }

    public async Task<PortfolioDeployment?> GetByIdAsync(Guid id)
    {
        return await _db.Set<PortfolioDeployment>().AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<IReadOnlyList<PortfolioDeployment>> GetAllAsync()
    {
        return await _db.Set<PortfolioDeployment>().AsNoTracking()
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<PortfolioDeployment?> GetByPortfolioIdAsync(Guid portfolioId)
    {
        return await _db.Set<PortfolioDeployment>().AsNoTracking()
            .FirstOrDefaultAsync(d => d.PortfolioId == portfolioId);
    }

    public async Task<PortfolioDeployment?> GetByConnectionIdAsync(Guid connectionId)
    {
        return await _db.Set<PortfolioDeployment>().AsNoTracking()
            .FirstOrDefaultAsync(d => d.ConnectionId == connectionId);
    }

    public async Task AddAsync(PortfolioDeployment deployment)
    {
        await _db.Set<PortfolioDeployment>().AddAsync(deployment);
    }

    public void Update(PortfolioDeployment deployment)
    {
        _db.Set<PortfolioDeployment>().Update(deployment);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _db.SaveChangesAsync();
    }
}
