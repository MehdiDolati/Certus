using Certus.Domain.Strategy.Aggregates;
using Certus.Domain.Strategy.Enums;

namespace Certus.Domain.Strategy.Repositories;

public interface IStrategyDefinitionRepository
{
    Task<StrategyDefinition?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<StrategyDefinition>> GetByPortfolioIdAsync(Guid portfolioId);
    Task<IReadOnlyList<StrategyDefinition>> GetByStatusAsync(StrategyStatus status);
    Task<IReadOnlyList<StrategyDefinition>> GetAllAsync();
    Task AddAsync(StrategyDefinition strategy);
    void Update(StrategyDefinition strategy);
    Task<int> SaveChangesAsync();
}
