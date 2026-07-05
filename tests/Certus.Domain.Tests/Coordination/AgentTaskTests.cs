using Certus.Domain.Coordination.Aggregates;
using Certus.Domain.Coordination.Enums;
using Certus.Domain.Coordination.ValueObjects;
using FluentAssertions;
using TaskStatus = Certus.Domain.Coordination.Enums.TaskStatus;

namespace Certus.Domain.Tests.Coordination;

public class AgentTaskTests
{
    [Fact]
    public void AgentTask_Should_Be_Created_With_Correct_Defaults()
    {
        var id = Guid.NewGuid();
        var agentType = AgentType.StrategyDesign;
        var description = "Design portfolio rebalance strategy";
        var priority = new TaskPriority(7);

        var task = new AgentTask(id, agentType, description, priority);

        task.Id.Should().Be(id);
        task.AssignedAgent.Should().Be(agentType);
        task.Description.Should().Be(description);
        task.Priority.Should().Be(priority);
        task.Status.Should().Be(TaskStatus.Pending);
        task.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        task.StartedAt.Should().BeNull();
        task.CompletedAt.Should().BeNull();
        task.Result.Should().BeNull();
        task.ErrorMessage.Should().BeNull();
        task.Dependencies.Should().BeEmpty();
    }

    [Fact]
    public void AgentTask_Start_Should_Set_Status_To_InProgress()
    {
        var task = CreateDefaultTask();

        task.Start();

        task.Status.Should().Be(TaskStatus.InProgress);
        task.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void AgentTask_Complete_Should_Set_Status_And_Result()
    {
        var task = CreateDefaultTask();
        task.Start();

        task.Complete("Strategy designed successfully");

        task.Status.Should().Be(TaskStatus.Completed);
        task.Result.Should().Be("Strategy designed successfully");
        task.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void AgentTask_Fail_Should_Set_Status_And_ErrorMessage()
    {
        var task = CreateDefaultTask();
        task.Start();

        task.Fail("API rate limit exceeded");

        task.Status.Should().Be(TaskStatus.Failed);
        task.ErrorMessage.Should().Be("API rate limit exceeded");
        task.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void AgentTask_Cancel_Should_Set_Status_And_CompletedAt()
    {
        var task = CreateDefaultTask();

        task.Cancel();

        task.Status.Should().Be(TaskStatus.Cancelled);
        task.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void AgentTask_AddDependency_Should_Add_To_Collection()
    {
        var task = CreateDefaultTask();
        var dependencyTaskId = Guid.NewGuid();

        task.AddDependency(dependencyTaskId);

        task.Dependencies.Should().HaveCount(1);
        task.Dependencies[0].TaskId.Should().Be(task.Id);
        task.Dependencies[0].DependsOnTaskId.Should().Be(dependencyTaskId);
    }

    [Fact]
    public void AgentTask_AddMultipleDependencies_Should_All_Be_Added()
    {
        var task = CreateDefaultTask();

        task.AddDependency(Guid.NewGuid());
        task.AddDependency(Guid.NewGuid());
        task.AddDependency(Guid.NewGuid());

        task.Dependencies.Should().HaveCount(3);
    }

    [Fact]
    public void AgentTask_Dependencies_Should_Be_ReadOnly()
    {
        var task = CreateDefaultTask();

        task.Dependencies.Should().BeAssignableTo<IReadOnlyList<Domain.Coordination.Entities.TaskDependency>>();
    }

    [Fact]
    public void AgentTask_Equality_Should_Be_Based_On_Id()
    {
        var id = Guid.NewGuid();
        var task1 = new AgentTask(id, AgentType.Meta, "Task 1", new TaskPriority(5));
        var task2 = new AgentTask(id, AgentType.Meta, "Task 2", new TaskPriority(8));

        task1.Should().Be(task2);
    }

    private static AgentTask CreateDefaultTask() =>
        new(Guid.NewGuid(), AgentType.MarketIntelligence, "Gather market data", new TaskPriority(5));
}
