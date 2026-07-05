using Certus.Domain.Evaluation.Entities;

namespace Certus.Domain.Evaluation.Repositories;

public interface IStrategyEvaluationRepository
{
    Task<IReadOnlyList<StrategyEvaluation>> GetByStrategyAsync(Guid strategyId, int count = 10);
    Task<StrategyEvaluation?> GetLatestAsync(Guid strategyId);
    Task AddAsync(StrategyEvaluation evaluation);
    Task<int> SaveChangesAsync();
}
