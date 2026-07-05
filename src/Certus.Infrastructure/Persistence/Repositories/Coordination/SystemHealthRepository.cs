using Certus.Domain.Coordination.Aggregates;
using Certus.Domain.Coordination.Entities;
using Certus.Domain.Coordination.Repositories;

namespace Certus.Infrastructure.Persistence.Repositories.Coordination;

public class SystemHealthRepository : ISystemHealthRepository
{
    private readonly CertusDbContext _db;

    public SystemHealthRepository(CertusDbContext db)
    {
        _db = db;
    }

    public Task<SystemHealth?> GetCurrentAsync()
    {
        return Task.FromResult<SystemHealth?>(null);
    }

    public Task<SystemHealth?> GetByIdAsync(Guid id)
    {
        return Task.FromResult<SystemHealth?>(null);
    }

    public Task AddAsync(SystemHealth health)
    {
        return Task.CompletedTask;
    }

    public void Update(SystemHealth health) { }

    public Task<IReadOnlyList<AgentState>> GetAgentStatesAsync(Guid systemHealthId)
    {
        return Task.FromResult<IReadOnlyList<AgentState>>(new List<AgentState>());
    }

    public Task<int> SaveChangesAsync()
    {
        return Task.FromResult(0);
    }
}
