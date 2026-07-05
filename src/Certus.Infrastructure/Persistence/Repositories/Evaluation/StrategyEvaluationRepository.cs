using Certus.Domain.Evaluation.Entities;
using Certus.Domain.Evaluation.Repositories;

namespace Certus.Infrastructure.Persistence.Repositories.Evaluation;

public class StrategyEvaluationRepository : IStrategyEvaluationRepository
{
    private readonly CertusDbContext _db;

    public StrategyEvaluationRepository(CertusDbContext db)
    {
        _db = db;
    }

    public Task<IReadOnlyList<StrategyEvaluation>> GetByStrategyAsync(Guid strategyId, int count = 10)
    {
        return Task.FromResult<IReadOnlyList<StrategyEvaluation>>(new List<StrategyEvaluation>());
    }

    public Task<StrategyEvaluation?> GetLatestAsync(Guid strategyId)
    {
        return Task.FromResult<StrategyEvaluation?>(null);
    }

    public Task AddAsync(StrategyEvaluation evaluation)
    {
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync()
    {
        return Task.FromResult(0);
    }
}
