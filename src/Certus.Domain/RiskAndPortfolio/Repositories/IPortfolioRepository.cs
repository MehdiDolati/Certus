using Certus.Domain.RiskAndPortfolio.Aggregates;
using Certus.Domain.RiskAndPortfolio.Enums;

namespace Certus.Domain.RiskAndPortfolio.Repositories;

public interface IPortfolioRepository
{
    Task<Portfolio?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<Portfolio>> GetAllAsync();
    Task<IReadOnlyList<Portfolio>> GetByStatusAsync(PortfolioStatus status);
    Task AddAsync(Portfolio portfolio);
    void Update(Portfolio portfolio);
    Task<int> SaveChangesAsync();
}
