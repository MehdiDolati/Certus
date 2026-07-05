using Certus.Domain.Strategy.Entities;

namespace Certus.Domain.Strategy.Repositories;

public interface IBacktestRepository
{
    Task<BacktestRun?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<BacktestRun>> GetByStrategyIdAsync(Guid strategyId);
    Task AddAsync(BacktestRun run);
    void Update(BacktestRun run);
    Task<int> SaveChangesAsync();
}
