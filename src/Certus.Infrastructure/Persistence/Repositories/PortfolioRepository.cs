using Certus.Domain.RiskAndPortfolio.Aggregates;
using Certus.Domain.RiskAndPortfolio.Enums;
using Certus.Domain.RiskAndPortfolio.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Certus.Infrastructure.Persistence.Repositories;

public class PortfolioRepository : IPortfolioRepository
{
    private readonly CertusDbContext _db;

    public PortfolioRepository(CertusDbContext db)
    {
        _db = db;
    }

    public async Task<Portfolio?> GetByIdAsync(Guid id)
    {
        return await _db.Portfolios.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IReadOnlyList<Portfolio>> GetAllAsync()
    {
        return await _db.Portfolios.AsNoTracking().ToListAsync();
    }

    public async Task<IReadOnlyList<Portfolio>> GetByStatusAsync(PortfolioStatus status)
    {
        return await _db.Portfolios.AsNoTracking()
            .Where(p => p.Status == status)
            .ToListAsync();
    }

    public async Task AddAsync(Portfolio portfolio)
    {
        await _db.Portfolios.AddAsync(portfolio);
    }

    public void Update(Portfolio portfolio)
    {
        _db.Portfolios.Update(portfolio);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _db.SaveChangesAsync();
    }
}
