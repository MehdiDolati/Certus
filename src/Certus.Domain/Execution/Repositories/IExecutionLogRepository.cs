using Certus.Domain.Execution.Entities;

namespace Certus.Domain.Execution.Repositories;

public interface IExecutionLogRepository
{
    Task AddAsync(ExecutionLog log);
    Task<IReadOnlyList<ExecutionLog>> GetByTradeIdAsync(Guid tradeId);
    Task<int> SaveChangesAsync();
}
