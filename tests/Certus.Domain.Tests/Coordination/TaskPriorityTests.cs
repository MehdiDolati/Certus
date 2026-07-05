using Certus.Domain.Coordination.ValueObjects;
using FluentAssertions;

namespace Certus.Domain.Tests.Coordination;

public class TaskPriorityTests
{
    [Fact]
    public void TaskPriority_Should_Store_Value_And_IsUrgent()
    {
        var priority = new TaskPriority(7, true);

        priority.Value.Should().Be(7);
        priority.IsUrgent.Should().BeTrue();
    }

    [Fact]
    public void TaskPriority_IsUrgent_Should_Default_To_False()
    {
        var priority = new TaskPriority(5);

        priority.IsUrgent.Should().BeFalse();
    }

    [Fact]
    public void TaskPriority_Should_Throw_When_Value_Below_1()
    {
        Action act = () => new TaskPriority(0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void TaskPriority_Should_Throw_When_Value_Above_10()
    {
        Action act = () => new TaskPriority(11);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void TaskPriority_Should_Accept_Boundary_Values()
    {
        var low = new TaskPriority(1);
        var high = new TaskPriority(10);

        low.Value.Should().Be(1);
        high.Value.Should().Be(10);
    }

    [Fact]
    public void TaskPriority_Low_Should_Have_Value_3()
    {
        TaskPriority.Low.Value.Should().Be(3);
        TaskPriority.Low.IsUrgent.Should().BeFalse();
    }

    [Fact]
    public void TaskPriority_Normal_Should_Have_Value_5()
    {
        TaskPriority.Normal.Value.Should().Be(5);
        TaskPriority.Normal.IsUrgent.Should().BeFalse();
    }

    [Fact]
    public void TaskPriority_High_Should_Have_Value_8()
    {
        TaskPriority.High.Value.Should().Be(8);
        TaskPriority.High.IsUrgent.Should().BeFalse();
    }

    [Fact]
    public void TaskPriority_Critical_Should_Have_Value_10_And_Be_Urgent()
    {
        TaskPriority.Critical.Value.Should().Be(10);
        TaskPriority.Critical.IsUrgent.Should().BeTrue();
    }

    [Fact]
    public void TaskPriority_Equality_Should_Be_Based_On_Value_And_IsUrgent()
    {
        var p1 = new TaskPriority(5, false);
        var p2 = new TaskPriority(5, false);

        p1.Should().Be(p2);
    }

    [Fact]
    public void TaskPriority_Different_IsUrgent_Should_Not_Be_Equal()
    {
        var p1 = new TaskPriority(5, false);
        var p2 = new TaskPriority(5, true);

        p1.Should().NotBe(p2);
    }
}
