using Certus.Domain.Coordination.Aggregates;
using Certus.Domain.Coordination.Entities;

namespace Certus.Domain.Coordination.Repositories;

public interface ISystemHealthRepository
{
    Task<SystemHealth?> GetCurrentAsync();
    Task<SystemHealth?> GetByIdAsync(Guid id);
    Task AddAsync(SystemHealth health);
    void Update(SystemHealth health);
    Task<IReadOnlyList<AgentState>> GetAgentStatesAsync(Guid systemHealthId);
    Task<int> SaveChangesAsync();
}
