using Certus.Domain.RiskAndPortfolio.Entities;

namespace Certus.Domain.RiskAndPortfolio.Repositories;

public interface IRiskRepository
{
    Task<IReadOnlyList<RiskAssessment>> GetLatestByPortfolioAsync(Guid portfolioId, int count = 10);
    Task<RiskAssessment?> GetCurrentAsync(Guid portfolioId);
    Task AddAsync(RiskAssessment assessment);
    Task<int> SaveChangesAsync();
}
