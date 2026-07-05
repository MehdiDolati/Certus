using Certus.Domain.Evaluation.Aggregates;

namespace Certus.Domain.Evaluation.Repositories;

public interface IPerformanceReportRepository
{
    Task<PerformanceReport?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<PerformanceReport>> GetByPortfolioAsync(Guid portfolioId, DateTime from, DateTime to);
    Task<PerformanceReport?> GetLatestAsync(Guid portfolioId);
    Task AddAsync(PerformanceReport report);
    void Update(PerformanceReport report);
    Task<int> SaveChangesAsync();
}
