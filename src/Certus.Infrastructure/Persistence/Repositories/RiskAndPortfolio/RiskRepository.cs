using Certus.Domain.RiskAndPortfolio.Entities;
using Certus.Domain.RiskAndPortfolio.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Certus.Infrastructure.Persistence.Repositories.RiskAndPortfolio;

public class RiskRepository : IRiskRepository
{
    private readonly CertusDbContext _db;

    public RiskRepository(CertusDbContext db)
    {
        _db = db;
    }

    public Task<IReadOnlyList<RiskAssessment>> GetLatestByPortfolioAsync(Guid portfolioId, int count = 10)
    {
        return Task.FromResult<IReadOnlyList<RiskAssessment>>(new List<RiskAssessment>());
    }

    public Task<RiskAssessment?> GetCurrentAsync(Guid portfolioId)
    {
        return Task.FromResult<RiskAssessment?>(null);
    }

    public Task AddAsync(RiskAssessment assessment)
    {
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync()
    {
        return Task.FromResult(0);
    }
}
