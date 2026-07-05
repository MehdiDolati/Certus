using Certus.Domain.Strategy.Entities;
using Certus.Domain.Strategy.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Certus.Infrastructure.Persistence.Repositories.Strategy;

public class BacktestRepository : IBacktestRepository
{
    private readonly CertusDbContext _db;

    public BacktestRepository(CertusDbContext db)
    {
        _db = db;
    }

    public Task<BacktestRun?> GetByIdAsync(Guid id)
    {
        return Task.FromResult<BacktestRun?>(null);
    }

    public Task<IReadOnlyList<BacktestRun>> GetByStrategyIdAsync(Guid strategyId)
    {
        return Task.FromResult<IReadOnlyList<BacktestRun>>(new List<BacktestRun>());
    }

    public Task AddAsync(BacktestRun run)
    {
        return Task.CompletedTask;
    }

    public void Update(BacktestRun run) { }

    public Task<int> SaveChangesAsync()
    {
        return Task.FromResult(0);
    }
}
