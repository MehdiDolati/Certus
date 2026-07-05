using Certus.Application.Platform;
using FluentAssertions;

namespace Certus.Application.Tests.Platform;

public class DeviationCalculatorTests
{
    private readonly DeviationCalculator _sut = new();

    // AC-020: Given imported trades and expected trade outcomes, when calculating deviations, then deviation percentage and direction are computed
    [Fact]
    public void Calculate_Should_Compute_Deviation_Percentage()
    {
        var actual = 150m;
        var expected = 100m;

        var result = _sut.Calculate(actual, expected);

        result.Percentage.Should().Be(50m);
    }

    [Fact]
    public void Calculate_Should_Compute_Negative_Deviation()
    {
        var actual = 50m;
        var expected = 100m;

        var result = _sut.Calculate(actual, expected);

        result.Percentage.Should().Be(-50m);
    }

    [Fact]
    public void Calculate_Should_Detect_Upward_Direction()
    {
        var result = _sut.Calculate(150m, 100m);

        result.Direction.Should().Be(DeviationDirection.Upward);
    }

    [Fact]
    public void Calculate_Should_Detect_Downward_Direction()
    {
        var result = _sut.Calculate(50m, 100m);

        result.Direction.Should().Be(DeviationDirection.Downward);
    }

    [Fact]
    public void Calculate_Should_Detect_Neutral_When_Zero()
    {
        var result = _sut.Calculate(100m, 100m);

        result.Direction.Should().Be(DeviationDirection.Neutral);
    }

    [Fact]
    public void Calculate_Should_Handle_Zero_Expected()
    {
        var result = _sut.Calculate(100m, 0m);

        result.Percentage.Should().Be(100m);
        result.Direction.Should().Be(DeviationDirection.Upward);
    }

    [Fact]
    public void Calculate_Should_Handle_Both_Zero()
    {
        var result = _sut.Calculate(0m, 0m);

        result.Percentage.Should().Be(0m);
        result.Direction.Should().Be(DeviationDirection.Neutral);
    }

    [Fact]
    public void CalculateTradeDeviation_Should_Compare_Actual_vs_Expected()
    {
        var actual = new TradeOutcome { PnL = 150m, Duration = TimeSpan.FromHours(2) };
        var expected = new TradeOutcome { PnL = 100m, Duration = TimeSpan.FromHours(1) };

        var result = _sut.CalculateTradeDeviation(actual, expected);

        result.PnLDeviation.Percentage.Should().Be(50m);
        result.PnLDeviation.Direction.Should().Be(DeviationDirection.Upward);
        result.DurationDeviation.Percentage.Should().Be(100m);
    }

    [Fact]
    public void CalculateTradeDeviation_Should_Handle_Loss_Trade()
    {
        var actual = new TradeOutcome { PnL = -50m, Duration = TimeSpan.FromMinutes(30) };
        var expected = new TradeOutcome { PnL = 50m, Duration = TimeSpan.FromHours(1) };

        var result = _sut.CalculateTradeDeviation(actual, expected);

        result.PnLDeviation.Percentage.Should().Be(-200m);
        result.PnLDeviation.Direction.Should().Be(DeviationDirection.Downward);
    }
}
