using Certus.Domain.Coordination.Aggregates;
using Certus.Domain.Coordination.Enums;

namespace Certus.Domain.Coordination.Repositories;

using TaskStatus = Certus.Domain.Coordination.Enums.TaskStatus;

public interface IAgentTaskRepository
{
    Task<AgentTask?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<AgentTask>> GetByStatusAsync(TaskStatus status);
    Task<IReadOnlyList<AgentTask>> GetByAgentAsync(AgentType agent);
    Task AddAsync(AgentTask task);
    void Update(AgentTask task);
    Task<int> SaveChangesAsync();
}
