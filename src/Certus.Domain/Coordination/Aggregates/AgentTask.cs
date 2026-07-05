using Certus.Domain.Coordination.Entities;
using Certus.Domain.Coordination.Enums;
using Certus.Domain.Coordination.ValueObjects;
using Certus.Domain.SharedKernel;

namespace Certus.Domain.Coordination.Aggregates;

using TaskStatus = Certus.Domain.Coordination.Enums.TaskStatus;

public class AgentTask : AggregateRoot
{
    private readonly List<TaskDependency> _dependencies = [];

    public IReadOnlyList<TaskDependency> Dependencies => _dependencies.AsReadOnly();

    public AgentType AssignedAgent { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public TaskStatus Status { get; private set; }
    public TaskPriority Priority { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? Result { get; private set; }
    public string? ErrorMessage { get; private set; }

    private AgentTask() { }

    public AgentTask(
        Guid id,
        AgentType assignedAgent,
        string description,
        TaskPriority priority) : base(id)
    {
        AssignedAgent = assignedAgent;
        Description = description;
        Priority = priority;
        Status = TaskStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void Start()
    {
        Status = TaskStatus.InProgress;
        StartedAt = DateTime.UtcNow;
    }

    public void Complete(string result)
    {
        Status = TaskStatus.Completed;
        Result = result;
        CompletedAt = DateTime.UtcNow;
    }

    public void Fail(string errorMessage)
    {
        Status = TaskStatus.Failed;
        ErrorMessage = errorMessage;
        CompletedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = TaskStatus.Cancelled;
        CompletedAt = DateTime.UtcNow;
    }

    public void AddDependency(Guid dependencyTaskId)
    {
        var dependency = new TaskDependency(Guid.NewGuid(), Id, dependencyTaskId);
        _dependencies.Add(dependency);
    }
}
