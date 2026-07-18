using Certus.Domain.Platform.Aggregates;

namespace Certus.Domain.Platform.Repositories;

public interface IPortfolioDeploymentRepository
{
    Task<PortfolioDeployment?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<PortfolioDeployment>> GetAllAsync();
    Task<PortfolioDeployment?> GetByPortfolioIdAsync(Guid portfolioId);
    Task<PortfolioDeployment?> GetByConnectionIdAsync(Guid connectionId);
    Task AddAsync(PortfolioDeployment deployment);
    void Update(PortfolioDeployment deployment);
    Task<int> SaveChangesAsync();
}
