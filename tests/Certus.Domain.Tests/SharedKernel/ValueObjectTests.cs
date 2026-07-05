using Certus.Domain.SharedKernel;
using FluentAssertions;

namespace Certus.Domain.Tests.SharedKernel;

public class ValueObjectTests
{
    private class TestValueObject(int value) : ValueObject
    {
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return value;
        }
    }

    [Fact]
    public void Equals_Same_Components_Should_Be_True()
    {
        var left = new TestValueObject(42);
        var right = new TestValueObject(42);
        left.Equals(right).Should().BeTrue();
    }

    [Fact]
    public void Equals_Different_Components_Should_Be_False()
    {
        var left = new TestValueObject(42);
        var right = new TestValueObject(99);
        left.Equals(right).Should().BeFalse();
    }

    [Fact]
    public void Equals_Null_Should_Be_False()
    {
        var vo = new TestValueObject(42);
        vo.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Equals_Different_Type_Should_Be_False()
    {
        var vo = new TestValueObject(42);
        vo.Equals("not a value object").Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_Same_Components_Should_Be_Same()
    {
        var left = new TestValueObject(42);
        var right = new TestValueObject(42);
        left.GetHashCode().Should().Be(right.GetHashCode());
    }

    [Fact]
    public void Operator_Equals_Should_Work()
    {
        var left = new TestValueObject(42);
        var right = new TestValueObject(42);
        (left == right).Should().BeTrue();
    }

    [Fact]
    public void Operator_NotEquals_Should_Work()
    {
        var left = new TestValueObject(42);
        var right = new TestValueObject(99);
        (left != right).Should().BeTrue();
    }
}
