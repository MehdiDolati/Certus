using Certus.Domain.Evaluation.Entities;

namespace Certus.Domain.Evaluation.Repositories;

public interface IPerformanceSnapshotRepository
{
    Task<IReadOnlyList<PerformanceSnapshot>> GetByPortfolioIdAsync(Guid portfolioId, DateTime? from, DateTime? to);
    Task<IReadOnlyList<PerformanceSnapshot>> GetByStrategyIdAsync(Guid strategyId, DateTime? from, DateTime? to);
    Task AddAsync(PerformanceSnapshot snapshot);
    Task AddRangeAsync(IEnumerable<PerformanceSnapshot> snapshots);
    Task<int> SaveChangesAsync();
}
