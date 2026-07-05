using Certus.Domain.Market.ValueObjects;
using Certus.Domain.Strategy.Aggregates;
using Certus.Domain.Strategy.Entities;
using Certus.Domain.Strategy.Enums;
using Certus.Domain.Strategy.Events;
using Certus.Domain.Strategy.ValueObjects;
using Certus.Domain.SharedKernel;
using FluentAssertions;

namespace Certus.Domain.Tests;

public class StrategyTests
{
    [Fact]
    public void Strategy_Should_Be_Created_With_Correct_Defaults()
    {
        var id = Guid.NewGuid();
        var strategy = new StrategyDefinition(id, "Momentum", new StrategyType(StrategyCategory.Momentum), 0.25m, "Test");

        strategy.Id.Should().Be(id);
        strategy.Name.Should().Be("Momentum");
        strategy.Type.Should().Be(new StrategyType(StrategyCategory.Momentum));
        strategy.TargetReturn.Should().Be(0.25m);
        strategy.Description.Should().Be("Test");
        strategy.Status.Should().Be(StrategyStatus.Draft);
        strategy.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        strategy.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Strategy_Should_Have_Empty_Collections_Initially()
    {
        var strategy = CreateStrategy();

        strategy.Parameters.Should().BeEmpty();
        strategy.BacktestRuns.Should().BeEmpty();
        strategy.Slots.Should().BeEmpty();
    }

    [Fact]
    public void Strategy_Should_Have_Null_RiskProfile_And_PredictedPerformance()
    {
        var strategy = CreateStrategy();

        strategy.RiskProfile.Should().BeNull();
        strategy.PredictedPerformance.Should().BeNull();
    }

    [Fact]
    public void Strategy_IsActive_Should_Return_True_When_Status_Is_Active()
    {
        var strategy = CreateStrategy();
        strategy.Activate();
        strategy.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData(StrategyStatus.Draft)]
    [InlineData(StrategyStatus.Paused)]
    [InlineData(StrategyStatus.Retired)]
    [InlineData(StrategyStatus.Backtesting)]
    public void Strategy_IsActive_Should_Return_False_For_Non_Active_Status(StrategyStatus status)
    {
        var strategy = CreateStrategy();
        SetStrategyStatus(strategy, status);
        strategy.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Strategy_CanBeActivated_From_Draft_Should_Be_True()
    {
        var strategy = CreateStrategy();

        strategy.CanBeActivated.Should().BeTrue();
    }

    [Fact]
    public void Strategy_CanBeActivated_From_Backtesting_Should_Be_True()
    {
        var strategy = CreateStrategy();
        SetStrategyStatus(strategy, StrategyStatus.Backtesting);

        strategy.CanBeActivated.Should().BeTrue();
    }

    [Theory]
    [InlineData(StrategyStatus.Active)]
    [InlineData(StrategyStatus.Paused)]
    [InlineData(StrategyStatus.Retired)]
    public void Strategy_CanBeActivated_Should_Be_False_For_Non_Activatable_Statuses(StrategyStatus status)
    {
        var strategy = CreateStrategy();
        SetStrategyStatus(strategy, status);

        strategy.CanBeActivated.Should().BeFalse();
    }

    [Fact]
    public void UpdateDetails_Should_Update_All_Fields()
    {
        var strategy = CreateStrategy();
        var beforeUpdate = DateTime.UtcNow;

        strategy.UpdateDetails("NewName", new StrategyType(StrategyCategory.Arbitrage), 0.3m, "New desc");

        strategy.Name.Should().Be("NewName");
        strategy.Type.Should().Be(new StrategyType(StrategyCategory.Arbitrage));
        strategy.TargetReturn.Should().Be(0.3m);
        strategy.Description.Should().Be("New desc");
        strategy.UpdatedAt.Should().BeOnOrAfter(beforeUpdate);
    }

    [Fact]
    public void AddParameter_Should_Add_To_Collection()
    {
        var strategy = CreateStrategy();
        var param = new StrategyParameter(Guid.NewGuid(), "Lookback", "int", 20m, 5m, 100m);

        strategy.AddParameter(param);

        strategy.Parameters.Should().ContainSingle().Which.Should().Be(param);
    }

    [Fact]
    public void AddParameter_Should_Update_UpdatedAt()
    {
        var strategy = CreateStrategy();
        var beforeAdd = DateTime.UtcNow;

        strategy.AddParameter(new StrategyParameter(Guid.NewGuid(), "P", "int", 1m, 0m, 10m));

        strategy.UpdatedAt.Should().BeOnOrAfter(beforeAdd);
    }

    [Fact]
    public void RemoveParameter_Should_Remove_From_Collection()
    {
        var strategy = CreateStrategy();
        var paramId = Guid.NewGuid();
        strategy.AddParameter(new StrategyParameter(paramId, "P", "int", 1m, 0m, 10m));

        strategy.RemoveParameter(paramId);

        strategy.Parameters.Should().BeEmpty();
    }

    [Fact]
    public void RemoveParameter_NonExistent_Should_Do_Nothing()
    {
        var strategy = CreateStrategy();
        strategy.AddParameter(new StrategyParameter(Guid.NewGuid(), "P", "int", 1m, 0m, 10m));

        strategy.RemoveParameter(Guid.NewGuid());

        strategy.Parameters.Should().ContainSingle();
    }

    [Fact]
    public void AddBacktestRun_Should_Add_To_Collection()
    {
        var strategy = CreateStrategy();
        var run = new BacktestRun(Guid.NewGuid(), strategy.Id, CreateBacktestConfig());

        strategy.AddBacktestRun(run);

        strategy.BacktestRuns.Should().ContainSingle().Which.Should().Be(run);
    }

    [Fact]
    public void AssignToPortfolio_Should_Add_Slot()
    {
        var strategy = CreateStrategy();
        var portfolioId = Guid.NewGuid();
        var weight = new Weight(0.3m);

        strategy.AssignToPortfolio(portfolioId, weight);

        strategy.Slots.Should().ContainSingle();
        strategy.Slots[0].PortfolioId.Should().Be(portfolioId);
        strategy.Slots[0].Weight.Should().Be(weight);
    }

    [Fact]
    public void AssignToPortfolio_Duplicate_Should_Throw()
    {
        var strategy = CreateStrategy();
        var portfolioId = Guid.NewGuid();
        strategy.AssignToPortfolio(portfolioId, new Weight(0.3m));

        var act = () => strategy.AssignToPortfolio(portfolioId, new Weight(0.5m));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already assigned*");
    }

    [Fact]
    public void RemoveFromPortfolio_Should_Remove_Slot()
    {
        var strategy = CreateStrategy();
        var portfolioId = Guid.NewGuid();
        strategy.AssignToPortfolio(portfolioId, new Weight(0.3m));

        strategy.RemoveFromPortfolio(portfolioId);

        strategy.Slots.Should().BeEmpty();
    }

    [Fact]
    public void RemoveFromPortfolio_NonExistent_Should_Do_Nothing()
    {
        var strategy = CreateStrategy();
        strategy.AssignToPortfolio(Guid.NewGuid(), new Weight(0.3m));

        strategy.RemoveFromPortfolio(Guid.NewGuid());

        strategy.Slots.Should().ContainSingle();
    }

    [Fact]
    public void Activate_From_Draft_Should_Set_Status_Active()
    {
        var strategy = CreateStrategy();

        strategy.Activate();

        strategy.Status.Should().Be(StrategyStatus.Active);
    }

    [Fact]
    public void Activate_From_Draft_Should_Raise_StrategyActivated_Event()
    {
        var strategy = CreateStrategy();

        strategy.Activate();

        strategy.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<StrategyActivated>();
    }

    [Fact]
    public void Activate_From_Backtesting_Should_Succeed()
    {
        var strategy = CreateStrategy();
        SetStrategyStatus(strategy, StrategyStatus.Backtesting);

        strategy.Activate();

        strategy.Status.Should().Be(StrategyStatus.Active);
    }

    [Theory]
    [InlineData(StrategyStatus.Active)]
    [InlineData(StrategyStatus.Paused)]
    [InlineData(StrategyStatus.Retired)]
    public void Activate_From_Invalid_Status_Should_Throw(StrategyStatus status)
    {
        var strategy = CreateStrategy();
        SetStrategyStatus(strategy, status);

        var act = () => strategy.Activate();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Pause_From_Active_Should_Set_Status_Paused()
    {
        var strategy = CreateStrategy();
        strategy.Activate();

        strategy.Pause();

        strategy.Status.Should().Be(StrategyStatus.Paused);
    }

    [Fact]
    public void Pause_From_Active_Should_Raise_StrategyDeactivated_Event()
    {
        var strategy = CreateStrategy();
        strategy.Activate();

        strategy.Pause();

        strategy.DomainEvents.Should()
            .Contain(e => e is StrategyDeactivated)
            .And.Contain(e => e is StrategyActivated);
    }

    [Fact]
    public void Pause_From_Non_Active_Should_Throw()
    {
        var strategy = CreateStrategy();

        var act = () => strategy.Pause();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*only pause an active strategy*");
    }

    [Fact]
    public void Retire_From_Draft_Should_Set_Status_Retired()
    {
        var strategy = CreateStrategy();

        strategy.Retire();

        strategy.Status.Should().Be(StrategyStatus.Retired);
    }

    [Fact]
    public void Retire_From_Retired_Should_Throw()
    {
        var strategy = CreateStrategy();
        strategy.Retire();

        var act = () => strategy.Retire();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already retired*");
    }

    [Fact]
    public void Retire_Should_Raise_StrategyDeactivated_Event()
    {
        var strategy = CreateStrategy();

        strategy.Retire();

        strategy.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<StrategyDeactivated>();
    }

    [Fact]
    public void SetRiskProfile_Should_Set_Profile()
    {
        var strategy = CreateStrategy();
        var profile = new RiskProfile(0.2m, 0.3m, 0.15m);

        strategy.SetRiskProfile(profile);

        strategy.RiskProfile.Should().Be(profile);
    }

    [Fact]
    public void SetPredictedPerformance_Should_Set_Interval()
    {
        var strategy = CreateStrategy();
        var interval = new ConfidenceInterval(0.1m, 0.3m, 0.95m);

        strategy.SetPredictedPerformance(interval);

        strategy.PredictedPerformance.Should().Be(interval);
    }

    private static StrategyDefinition CreateStrategy() =>
        new(Guid.NewGuid(), "Test", new StrategyType(StrategyCategory.Momentum), 0.2m);

    private static void SetStrategyStatus(StrategyDefinition strategy, StrategyStatus status)
    {
        switch (status)
        {
            case StrategyStatus.Active:
                strategy.Activate();
                break;
            case StrategyStatus.Paused:
                strategy.Activate();
                strategy.Pause();
                break;
            case StrategyStatus.Retired:
                strategy.Retire();
                break;
            case StrategyStatus.Backtesting:
                // Use reflection or a helper to set Backtesting status
                // Since there's no public method, we use the private setter via reflection
                typeof(StrategyDefinition)
                    .GetProperty(nameof(StrategyDefinition.Status))!
                    .SetValue(strategy, StrategyStatus.Backtesting);
                break;
            case StrategyStatus.Draft:
                // Already the default
                break;
        }
    }

    private static BacktestConfig CreateBacktestConfig()
    {
        return new BacktestConfig(
            new DateTime(2024, 1, 1),
            new DateTime(2024, 12, 31),
            new Money(100_000m, Currency.USD),
            new List<Symbol> { new("BTCUSD", AssetClass.Crypto) },
            new TimeFrame("1h"));
    }
}
