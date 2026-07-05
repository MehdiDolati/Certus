using Certus.Domain.RiskAndPortfolio.Aggregates;
using Certus.Domain.RiskAndPortfolio.Enums;
using Certus.Domain.SharedKernel;
using FluentAssertions;

namespace Certus.Domain.Tests;

public class PortfolioTests
{
    [Fact]
    public void Portfolio_Should_Be_Created_With_Correct_Defaults()
    {
        var id = Guid.NewGuid();
        var portfolio = new Portfolio(id, "Test Fund", 0.15m, 1.5m, new Money(1_000_000m, Currency.USD), "Desc");

        portfolio.Id.Should().Be(id);
        portfolio.Name.Should().Be("Test Fund");
        portfolio.Description.Should().Be("Desc");
        portfolio.TargetReturn.Should().Be(0.15m);
        portfolio.TargetSharpe.Should().Be(1.5m);
        portfolio.AllocatedCapital.Amount.Should().Be(1_000_000m);
        portfolio.Status.Should().Be(PortfolioStatus.Active);
    }

    [Fact]
    public void Portfolio_IsActive_Should_Return_True_When_Status_Is_Active()
    {
        var portfolio = CreatePortfolio();
        portfolio.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData(PortfolioStatus.Paused)]
    [InlineData(PortfolioStatus.Closed)]
    public void Portfolio_IsActive_Should_Return_False_When_Status_Is_Not_Active(PortfolioStatus status)
    {
        var portfolio = CreatePortfolio();
        if (status == PortfolioStatus.Paused)
            portfolio.Pause();
        else
            portfolio.Close();
        portfolio.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Portfolio_CanAddStrategy_Should_Return_True_Only_When_Active()
    {
        var active = CreatePortfolio();
        var paused = CreatePortfolio();
        paused.Pause();
        var closed = CreatePortfolio();
        closed.Close();

        active.CanAddStrategy().Should().BeTrue();
        paused.CanAddStrategy().Should().BeFalse();
        closed.CanAddStrategy().Should().BeFalse();
    }

    [Fact]
    public void Portfolio_AllocatedCapitalInMillions_Should_Divide_By_One_Million()
    {
        var portfolio = new Portfolio(Guid.NewGuid(), "Fund", 0.1m, 1.0m, new Money(12_400_000m, Currency.USD));
        portfolio.AllocatedCapitalInMillions.Should().Be(12.4m);
    }

    private static Portfolio CreatePortfolio() =>
        new(Guid.NewGuid(), "Fund", 0.1m, 1.0m, new Money(1_000_000m, Currency.USD));
}
