using Certus.Domain.Strategy.Entities;
using FluentAssertions;

namespace Certus.Domain.Tests.Strategy;

public class StrategyParameterTests
{
    [Fact]
    public void Constructor_Should_Set_Properties_Correctly()
    {
        var id = Guid.NewGuid();

        var param = new StrategyParameter(id, "Lookback", "int", 20m, 5m, 100m);

        param.Id.Should().Be(id);
        param.Name.Should().Be("Lookback");
        param.Type.Should().Be("int");
        param.DefaultValue.Should().Be(20m);
        param.MinValue.Should().Be(5m);
        param.MaxValue.Should().Be(100m);
        param.CurrentValue.Should().Be(20m);
    }

    [Fact]
    public void Constructor_Should_Set_CurrentValue_To_Default()
    {
        var param = new StrategyParameter(Guid.NewGuid(), "Threshold", "decimal", 0.5m, 0m, 1m);

        param.CurrentValue.Should().Be(0.5m);
    }

    [Fact]
    public void UpdateValue_Should_Set_New_Value()
    {
        var param = new StrategyParameter(Guid.NewGuid(), "Lookback", "int", 20m, 5m, 100m);

        param.UpdateValue(50m);

        param.CurrentValue.Should().Be(50m);
    }

    [Fact]
    public void UpdateValue_At_Min_Should_Succeed()
    {
        var param = new StrategyParameter(Guid.NewGuid(), "Lookback", "int", 20m, 5m, 100m);

        param.UpdateValue(5m);

        param.CurrentValue.Should().Be(5m);
    }

    [Fact]
    public void UpdateValue_At_Max_Should_Succeed()
    {
        var param = new StrategyParameter(Guid.NewGuid(), "Lookback", "int", 20m, 5m, 100m);

        param.UpdateValue(100m);

        param.CurrentValue.Should().Be(100m);
    }

    [Fact]
    public void UpdateValue_Below_Min_Should_Throw()
    {
        var param = new StrategyParameter(Guid.NewGuid(), "Lookback", "int", 20m, 5m, 100m);

        var act = () => param.UpdateValue(4m);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void UpdateValue_Above_Max_Should_Throw()
    {
        var param = new StrategyParameter(Guid.NewGuid(), "Lookback", "int", 20m, 5m, 100m);

        var act = () => param.UpdateValue(101m);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ResetToDefault_Should_Restore_Default_Value()
    {
        var param = new StrategyParameter(Guid.NewGuid(), "Lookback", "int", 20m, 5m, 100m);
        param.UpdateValue(75m);

        param.ResetToDefault();

        param.CurrentValue.Should().Be(20m);
    }

    [Fact]
    public void ResetToDefault_Should_Do_Nothing_When_Already_Default()
    {
        var param = new StrategyParameter(Guid.NewGuid(), "Lookback", "int", 20m, 5m, 100m);

        param.ResetToDefault();

        param.CurrentValue.Should().Be(20m);
    }
}
