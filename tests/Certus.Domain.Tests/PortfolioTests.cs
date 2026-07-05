using Certus.Domain.RiskAndPortfolio.Aggregates;
using Certus.Domain.RiskAndPortfolio.Entities;
using Certus.Domain.RiskAndPortfolio.Enums;
using Certus.Domain.RiskAndPortfolio.Events;
using Certus.Domain.RiskAndPortfolio.ValueObjects;
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

    // AC-007: Given a portfolio with empty name, when creating, then validation error is thrown
    [Fact]
    public void Portfolio_Should_Throw_On_Empty_Name()
    {
        var act = () => new Portfolio(Guid.NewGuid(), "", 0.1m, 1.0m, new Money(1_000_000m, Currency.USD));
        act.Should().Throw<ArgumentException>();
    }

    // AC-008: Given a manually created portfolio, when checking status, then it is NOT Active
    [Fact]
    public void Manually_Created_Portfolio_Should_Not_Be_Active()
    {
        var portfolio = Portfolio.CreateManually("Test Fund", 0.1m, 1.0m, new Money(1_000_000m, Currency.USD));
        portfolio.IsActive.Should().BeFalse();
        portfolio.Status.Should().Be(PortfolioStatus.Draft);
    }

    // Platform reference tests
    [Fact]
    public void Portfolio_SetPlatformReference_Should_Set_Fields()
    {
        var portfolio = CreatePortfolio();
        var connectionId = Guid.NewGuid();

        portfolio.SetPlatformReference(connectionId, "MT4_12345");

        portfolio.PlatformConnectionId.Should().Be(connectionId);
        portfolio.ExternalPortfolioId.Should().Be("MT4_12345");
        portfolio.IsPlatformManaged.Should().BeTrue();
    }

    [Fact]
    public void Portfolio_ClearPlatformReference_Should_Clear_Fields()
    {
        var portfolio = CreatePortfolio();
        portfolio.SetPlatformReference(Guid.NewGuid(), "MT4_12345");

        portfolio.ClearPlatformReference();

        portfolio.PlatformConnectionId.Should().BeNull();
        portfolio.ExternalPortfolioId.Should().BeNull();
        portfolio.IsPlatformManaged.Should().BeFalse();
    }

    // MarketRegime tests
    [Fact]
    public void Portfolio_Should_Default_To_Normal_MarketRegime()
    {
        var portfolio = CreatePortfolio();
        portfolio.CurrentMarketRegime.Should().Be(MarketRegime.Normal);
    }

    [Fact]
    public void Portfolio_UpdateMarketRegime_Should_Change_Regime()
    {
        var portfolio = CreatePortfolio();

        portfolio.UpdateMarketRegime(MarketRegime.Crisis);

        portfolio.CurrentMarketRegime.Should().Be(MarketRegime.Crisis);
    }

    [Theory]
    [InlineData(MarketRegime.Normal)]
    [InlineData(MarketRegime.Trending)]
    [InlineData(MarketRegime.Ranging)]
    [InlineData(MarketRegime.Volatile)]
    [InlineData(MarketRegime.Crisis)]
    public void Portfolio_UpdateMarketRegime_Should_Accept_All_Regimes(MarketRegime regime)
    {
        var portfolio = CreatePortfolio();

        portfolio.UpdateMarketRegime(regime);

        portfolio.CurrentMarketRegime.Should().Be(regime);
    }

    // AdaptiveRiskParams tests
    [Fact]
    public void Portfolio_Should_Default_To_Null_AdaptiveRiskParams()
    {
        var portfolio = CreatePortfolio();
        portfolio.AdaptiveRiskParams.Should().BeNull();
    }

    [Fact]
    public void Portfolio_SetAdaptiveRiskParams_Should_Store_Params()
    {
        var portfolio = CreatePortfolio();
        var params_ = AdaptiveRiskParams.Aggressive;

        portfolio.SetAdaptiveRiskParams(params_);

        portfolio.AdaptiveRiskParams.Should().Be(params_);
    }

    [Fact]
    public void Portfolio_SetAdaptiveRiskParams_Can_Override()
    {
        var portfolio = CreatePortfolio();

        portfolio.SetAdaptiveRiskParams(AdaptiveRiskParams.Aggressive);
        portfolio.SetAdaptiveRiskParams(AdaptiveRiskParams.Crisis);

        portfolio.AdaptiveRiskParams.Should().Be(AdaptiveRiskParams.Crisis);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(MarketRegime.Normal)]
    [InlineData(MarketRegime.Crisis)]
    public void Portfolio_SetAdaptiveRiskParams_Should_Accept_Different_Regimes(MarketRegime? regime)
    {
        var portfolio = CreatePortfolio();
        var adaptParams = regime.HasValue
            ? new AdaptiveRiskParams(1m, 0.5m, regime.Value)
            : new AdaptiveRiskParams(1m, 0.5m, MarketRegime.Normal);

        portfolio.SetAdaptiveRiskParams(adaptParams);

        portfolio.AdaptiveRiskParams.Should().Be(adaptParams);
    }

    // RiskLimit tests
    [Fact]
    public void Portfolio_Should_Default_To_Empty_RiskLimits()
    {
        var portfolio = CreatePortfolio();
        portfolio.RiskLimits.Should().BeEmpty();
    }

    [Fact]
    public void Portfolio_AddRiskLimit_Should_Add_To_Collection()
    {
        var portfolio = CreatePortfolio();
        var limit = new RiskLimit(Guid.NewGuid(), portfolio.Id, RiskLimitType.MaxDrawdown, 0.2m);

        portfolio.AddRiskLimit(limit);

        portfolio.RiskLimits.Should().HaveCount(1);
        portfolio.RiskLimits[0].Should().Be(limit);
    }

    [Fact]
    public void Portfolio_AddRiskLimit_Multiple_Should_Add_All()
    {
        var portfolio = CreatePortfolio();
        var limit1 = new RiskLimit(Guid.NewGuid(), portfolio.Id, RiskLimitType.MaxDrawdown, 0.2m);
        var limit2 = new RiskLimit(Guid.NewGuid(), portfolio.Id, RiskLimitType.MaxLeverage, 3m);

        portfolio.AddRiskLimit(limit1);
        portfolio.AddRiskLimit(limit2);

        portfolio.RiskLimits.Should().HaveCount(2);
    }

    [Fact]
    public void Portfolio_RemoveRiskLimit_Should_Remove_From_Collection()
    {
        var portfolio = CreatePortfolio();
        var limit = new RiskLimit(Guid.NewGuid(), portfolio.Id, RiskLimitType.MaxDrawdown, 0.2m);
        portfolio.AddRiskLimit(limit);

        portfolio.RemoveRiskLimit(limit.Id);

        portfolio.RiskLimits.Should().BeEmpty();
    }

    [Fact]
    public void Portfolio_RemoveRiskLimit_NonExistent_Should_Do_Nothing()
    {
        var portfolio = CreatePortfolio();
        var limit = new RiskLimit(Guid.NewGuid(), portfolio.Id, RiskLimitType.MaxDrawdown, 0.2m);
        portfolio.AddRiskLimit(limit);

        portfolio.RemoveRiskLimit(Guid.NewGuid());

        portfolio.RiskLimits.Should().HaveCount(1);
    }

    [Fact]
    public void Portfolio_RemoveRiskLimit_Should_Not_Affect_Other_Limits()
    {
        var portfolio = CreatePortfolio();
        var limit1 = new RiskLimit(Guid.NewGuid(), portfolio.Id, RiskLimitType.MaxDrawdown, 0.2m);
        var limit2 = new RiskLimit(Guid.NewGuid(), portfolio.Id, RiskLimitType.MaxLeverage, 3m);
        portfolio.AddRiskLimit(limit1);
        portfolio.AddRiskLimit(limit2);

        portfolio.RemoveRiskLimit(limit1.Id);

        portfolio.RiskLimits.Should().HaveCount(1);
        portfolio.RiskLimits[0].Should().Be(limit2);
    }

    // AllocationRule tests
    [Fact]
    public void Portfolio_AddAllocationRule_Should_Add_To_Collection()
    {
        var portfolio = CreatePortfolio();
        var rule = new AllocationRule(Guid.NewGuid(), portfolio.Id, Guid.NewGuid(), 0.5m);

        portfolio.AddAllocationRule(rule);

        portfolio.AllocationRules.Should().HaveCount(1);
        portfolio.AllocationRules[0].Should().Be(rule);
    }

    // CapitalAllocation tests
    [Fact]
    public void Portfolio_AddCapitalAllocation_Should_Add_To_Collection()
    {
        var portfolio = CreatePortfolio();
        var allocation = new CapitalAllocation(
            Guid.NewGuid(), portfolio.Id, Guid.NewGuid(), new Money(100_000m, Currency.USD), 0.25m);

        portfolio.AddCapitalAllocation(allocation);

        portfolio.Allocations.Should().HaveCount(1);
        portfolio.Allocations[0].Should().Be(allocation);
    }

    // Domain event tests
    [Fact]
    public void Portfolio_Pause_Should_Raise_PortfolioStatusChanged_Event()
    {
        var portfolio = CreatePortfolio();

        portfolio.Pause();

        var evt = portfolio.DomainEvents.Should().ContainSingle()
            .Subject.Should().BeOfType<PortfolioStatusChanged>().Subject;

        evt.PortfolioId.Should().Be(portfolio.Id);
        evt.OldStatus.Should().Be(PortfolioStatus.Active);
        evt.NewStatus.Should().Be(PortfolioStatus.Paused);
    }

    [Fact]
    public void Portfolio_Resume_Should_Raise_PortfolioStatusChanged_Event()
    {
        var portfolio = CreatePortfolio();
        portfolio.Pause();

        portfolio.ClearDomainEvents();
        portfolio.Resume();

        var evt = portfolio.DomainEvents.Should().ContainSingle()
            .Subject.Should().BeOfType<PortfolioStatusChanged>().Subject;

        evt.OldStatus.Should().Be(PortfolioStatus.Paused);
        evt.NewStatus.Should().Be(PortfolioStatus.Active);
    }

    [Fact]
    public void Portfolio_Close_Should_Raise_PortfolioStatusChanged_Event()
    {
        var portfolio = CreatePortfolio();

        portfolio.Close();

        var evt = portfolio.DomainEvents.Should().ContainSingle()
            .Subject.Should().BeOfType<PortfolioStatusChanged>().Subject;

        evt.OldStatus.Should().Be(PortfolioStatus.Active);
        evt.NewStatus.Should().Be(PortfolioStatus.Closed);
    }

    [Fact]
    public void Portfolio_UpdateCapital_Should_Raise_CapitalReallocated_Event()
    {
        var portfolio = CreatePortfolio();
        var newCapital = new Money(2_000_000m, Currency.USD);

        portfolio.UpdateCapital(newCapital);

        var evt = portfolio.DomainEvents.Should().ContainSingle()
            .Subject.Should().BeOfType<CapitalReallocated>().Subject;

        evt.PortfolioId.Should().Be(portfolio.Id);
        evt.NewCapital.Should().Be(newCapital);
    }

    [Fact]
    public void Portfolio_Pause_From_Paused_Should_Throw()
    {
        var portfolio = CreatePortfolio();
        portfolio.Pause();

        var act = () => portfolio.Pause();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Portfolio_Resume_From_Active_Should_Throw()
    {
        var portfolio = CreatePortfolio();

        var act = () => portfolio.Resume();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Portfolio_Close_When_Already_Closed_Should_Throw()
    {
        var portfolio = CreatePortfolio();
        portfolio.Close();

        var act = () => portfolio.Close();

        act.Should().Throw<InvalidOperationException>();
    }

    private static Portfolio CreatePortfolio() =>
        new(Guid.NewGuid(), "Fund", 0.1m, 1.0m, new Money(1_000_000m, Currency.USD));
}
