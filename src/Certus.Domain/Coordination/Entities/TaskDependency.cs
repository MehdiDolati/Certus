using Certus.Domain.SharedKernel;

namespace Certus.Domain.Coordination.Entities;

public class TaskDependency : Entity
{
    public Guid TaskId { get; private set; }
    public Guid DependsOnTaskId { get; private set; }

    private TaskDependency() { }

    public TaskDependency(Guid id, Guid taskId, Guid dependsOnTaskId) : base(id)
    {
        TaskId = taskId;
        DependsOnTaskId = dependsOnTaskId;
    }
}
