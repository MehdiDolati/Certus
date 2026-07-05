using Certus.Domain.SharedKernel;
using FluentAssertions;

namespace Certus.Domain.Tests.SharedKernel;

public class EntityTests
{
    private class TestEntity(Guid id) : Entity(id) { }

    [Fact]
    public void Entity_Should_Have_Id()
    {
        var id = Guid.NewGuid();
        var entity = new TestEntity(id);
        entity.Id.Should().Be(id);
    }

    [Fact]
    public void Equals_Same_Id_Should_Be_True()
    {
        var id = Guid.NewGuid();
        var left = new TestEntity(id);
        var right = new TestEntity(id);
        left.Should().Be(right);
    }

    [Fact]
    public void Equals_Different_Id_Should_Be_False()
    {
        var left = new TestEntity(Guid.NewGuid());
        var right = new TestEntity(Guid.NewGuid());
        left.Should().NotBe(right);
    }

    [Fact]
    public void GetHashCode_Same_Id_Should_Be_Same()
    {
        var id = Guid.NewGuid();
        var left = new TestEntity(id);
        var right = new TestEntity(id);
        left.GetHashCode().Should().Be(right.GetHashCode());
    }

    [Fact]
    public void Operator_Equals_Should_Work()
    {
        var id = Guid.NewGuid();
        var left = new TestEntity(id);
        var right = new TestEntity(id);
        (left == right).Should().BeTrue();
    }

    [Fact]
    public void Operator_NotEquals_Should_Work()
    {
        var left = new TestEntity(Guid.NewGuid());
        var right = new TestEntity(Guid.NewGuid());
        (left != right).Should().BeTrue();
    }
}
