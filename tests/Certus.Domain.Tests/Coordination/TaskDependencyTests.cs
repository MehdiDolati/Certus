using Certus.Domain.Coordination.Entities;
using FluentAssertions;

namespace Certus.Domain.Tests.Coordination;

public class TaskDependencyTests
{
    [Fact]
    public void TaskDependency_Should_Be_Created_With_Correct_Values()
    {
        var id = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var dependsOnTaskId = Guid.NewGuid();

        var dependency = new TaskDependency(id, taskId, dependsOnTaskId);

        dependency.Id.Should().Be(id);
        dependency.TaskId.Should().Be(taskId);
        dependency.DependsOnTaskId.Should().Be(dependsOnTaskId);
    }

    [Fact]
    public void TaskDependency_Equality_Should_Be_Based_On_Id()
    {
        var id = Guid.NewGuid();
        var dep1 = new TaskDependency(id, Guid.NewGuid(), Guid.NewGuid());
        var dep2 = new TaskDependency(id, Guid.NewGuid(), Guid.NewGuid());

        dep1.Should().Be(dep2);
    }

    [Fact]
    public void TaskDependency_Different_Ids_Should_Not_Be_Equal()
    {
        var dep1 = new TaskDependency(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var dep2 = new TaskDependency(Guid.NewGuid(), dep1.TaskId, dep1.DependsOnTaskId);

        dep1.Should().NotBe(dep2);
    }
}
