using Certus.Domain.Evaluation.Aggregates;
using Certus.Domain.Evaluation.Repositories;

namespace Certus.Infrastructure.Persistence.Repositories.Evaluation;

public class PerformanceReportRepository : IPerformanceReportRepository
{
    private readonly CertusDbContext _db;

    public PerformanceReportRepository(CertusDbContext db)
    {
        _db = db;
    }

    public Task<PerformanceReport?> GetByIdAsync(Guid id)
    {
        return Task.FromResult<PerformanceReport?>(null);
    }

    public Task<IReadOnlyList<PerformanceReport>> GetByPortfolioAsync(Guid portfolioId, DateTime from, DateTime to)
    {
        return Task.FromResult<IReadOnlyList<PerformanceReport>>(new List<PerformanceReport>());
    }

    public Task<PerformanceReport?> GetLatestAsync(Guid portfolioId)
    {
        return Task.FromResult<PerformanceReport?>(null);
    }

    public Task AddAsync(PerformanceReport report)
    {
        return Task.CompletedTask;
    }

    public void Update(PerformanceReport report) { }

    public Task<int> SaveChangesAsync()
    {
        return Task.FromResult(0);
    }
}
