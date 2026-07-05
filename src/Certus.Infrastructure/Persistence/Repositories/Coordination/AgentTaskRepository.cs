using Certus.Domain.Coordination.Aggregates;
using Certus.Domain.Coordination.Enums;
using Certus.Domain.Coordination.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Certus.Infrastructure.Persistence.Repositories.Coordination;

using TaskStatus = Certus.Domain.Coordination.Enums.TaskStatus;

public class AgentTaskRepository : IAgentTaskRepository
{
    private readonly CertusDbContext _db;

    public AgentTaskRepository(CertusDbContext db)
    {
        _db = db;
    }

    public Task<AgentTask?> GetByIdAsync(Guid id)
    {
        return Task.FromResult<AgentTask?>(null);
    }

    public Task<IReadOnlyList<AgentTask>> GetByStatusAsync(TaskStatus status)
    {
        return Task.FromResult<IReadOnlyList<AgentTask>>(new List<AgentTask>());
    }

    public Task<IReadOnlyList<AgentTask>> GetByAgentAsync(AgentType agent)
    {
        return Task.FromResult<IReadOnlyList<AgentTask>>(new List<AgentTask>());
    }

    public Task AddAsync(AgentTask task)
    {
        return Task.CompletedTask;
    }

    public void Update(AgentTask task) { }

    public Task<int> SaveChangesAsync()
    {
        return Task.FromResult(0);
    }
}
