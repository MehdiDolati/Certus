using Certus.Domain.Evaluation.ValueObjects;
using FluentAssertions;

namespace Certus.Domain.Tests.Evaluation;

public class BenchmarkResultTests
{
    [Fact]
    public void BenchmarkResult_Should_Store_All_Properties()
    {
        var result = new BenchmarkResult(0.12m, 0.05m, 0.85m, 1.2m, 0.03m);

        result.BenchmarkReturn.Should().Be(0.12m);
        result.Alpha.Should().Be(0.05m);
        result.Beta.Should().Be(0.85m);
        result.InformationRatio.Should().Be(1.2m);
        result.TrackingError.Should().Be(0.03m);
    }

    [Theory]
    [InlineData(0.01, true)]
    [InlineData(0.0, false)]
    [InlineData(-0.1, false)]
    public void OutperformsBenchmark_Should_Return_Correctly(decimal alpha, bool expected)
    {
        var result = new BenchmarkResult(0.1m, alpha, 1.0m, 0.5m, 0.02m);

        result.OutperformsBenchmark.Should().Be(expected);
    }

    [Fact]
    public void BenchmarkResult_Should_Be_Equal_When_Values_Are_Same()
    {
        var a = new BenchmarkResult(0.1m, 0.05m, 0.8m, 1.0m, 0.02m);
        var b = new BenchmarkResult(0.1m, 0.05m, 0.8m, 1.0m, 0.02m);

        a.Should().Be(b);
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void BenchmarkResult_Should_Be_Unequal_When_Values_Differ()
    {
        var a = new BenchmarkResult(0.1m, 0.05m, 0.8m, 1.0m, 0.02m);
        var b = new BenchmarkResult(0.2m, 0.05m, 0.8m, 1.0m, 0.02m);

        a.Should().NotBe(b);
    }
}
