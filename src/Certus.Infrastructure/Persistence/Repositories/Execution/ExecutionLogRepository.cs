using Certus.Domain.Execution.Entities;
using Certus.Domain.Execution.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Certus.Infrastructure.Persistence.Repositories.Execution;

public class ExecutionLogRepository : IExecutionLogRepository
{
    private readonly CertusDbContext _db;

    public ExecutionLogRepository(CertusDbContext db)
    {
        _db = db;
    }

    public Task AddAsync(ExecutionLog log)
    {
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<ExecutionLog>> GetByTradeIdAsync(Guid tradeId)
    {
        return Task.FromResult<IReadOnlyList<ExecutionLog>>(new List<ExecutionLog>());
    }

    public Task<int> SaveChangesAsync()
    {
        return Task.FromResult(0);
    }
}
