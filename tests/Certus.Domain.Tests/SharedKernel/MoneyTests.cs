using Certus.Domain.SharedKernel;
using FluentAssertions;

namespace Certus.Domain.Tests.SharedKernel;

public class MoneyTests
{
    [Fact]
    public void Money_Should_Be_Created_With_Amount_And_Currency()
    {
        var money = new Money(100m, Currency.USD);
        money.Amount.Should().Be(100m);
        money.Currency.Should().Be(Currency.USD);
    }

    [Fact]
    public void Zero_Should_Return_Money_With_Zero_Amount()
    {
        var money = Money.Zero(Currency.EUR);
        money.Amount.Should().Be(0m);
        money.Currency.Should().Be(Currency.EUR);
    }

    [Fact]
    public void Addition_Same_Currency_Should_Work()
    {
        var left = new Money(100m, Currency.USD);
        var right = new Money(50m, Currency.USD);
        var result = left + right;
        result.Amount.Should().Be(150m);
        result.Currency.Should().Be(Currency.USD);
    }

    [Fact]
    public void Addition_Different_Currency_Should_Throw()
    {
        var left = new Money(100m, Currency.USD);
        var right = new Money(50m, Currency.EUR);
        var act = () => { var _ = left + right; };
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Subtraction_Same_Currency_Should_Work()
    {
        var left = new Money(100m, Currency.USD);
        var right = new Money(30m, Currency.USD);
        var result = left - right;
        result.Amount.Should().Be(70m);
    }

    [Fact]
    public void Subtraction_Different_Currency_Should_Throw()
    {
        var left = new Money(100m, Currency.USD);
        var right = new Money(30m, Currency.BTC);
        var act = () => { var _ = left - right; };
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Multiplication_Should_Work()
    {
        var money = new Money(100m, Currency.USD);
        var result = money * 2.5m;
        result.Amount.Should().Be(250m);
        result.Currency.Should().Be(Currency.USD);
    }

    [Fact]
    public void ToString_Should_Format_Correctly()
    {
        var money = new Money(1234.56m, Currency.USD);
        money.ToString().Should().Be("1234.56 USD");
    }

    [Fact]
    public void Money_Record_Should_Be_Equal_When_Same_Values()
    {
        var left = new Money(100m, Currency.USD);
        var right = new Money(100m, Currency.USD);
        left.Should().Be(right);
        left.GetHashCode().Should().Be(right.GetHashCode());
    }

    [Fact]
    public void Currency_All_Values_Should_Be_Covered()
    {
        Enum.GetValues<Currency>().Should().HaveCount(4);
    }
}
