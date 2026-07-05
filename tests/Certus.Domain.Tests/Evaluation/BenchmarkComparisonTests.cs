using Certus.Domain.Evaluation.Entities;
using Certus.Domain.Evaluation.Enums;
using Certus.Domain.Evaluation.ValueObjects;
using FluentAssertions;

namespace Certus.Domain.Tests.Evaluation;

public class BenchmarkComparisonTests
{
    [Fact]
    public void BenchmarkComparison_Should_Store_All_Properties()
    {
        var id = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        var result = new BenchmarkResult(0.1m, 0.05m, 0.85m, 1.2m, 0.03m);

        var comparison = new BenchmarkComparison(id, reportId, BenchmarkType.MarketIndex, result);

        comparison.Id.Should().Be(id);
        comparison.ReportId.Should().Be(reportId);
        comparison.BenchmarkType.Should().Be(BenchmarkType.MarketIndex);
        comparison.Result.Should().Be(result);
    }

    [Theory]
    [InlineData(BenchmarkType.BuyAndHold)]
    [InlineData(BenchmarkType.EqualWeight)]
    [InlineData(BenchmarkType.RiskFreeRate)]
    [InlineData(BenchmarkType.MarketIndex)]
    [InlineData(BenchmarkType.Custom)]
    public void BenchmarkComparison_Should_Accept_All_BenchmarkTypes(BenchmarkType type)
    {
        var result = new BenchmarkResult(0.1m, 0m, 1.0m, 0m, 0.01m);

        var comparison = new BenchmarkComparison(Guid.NewGuid(), Guid.NewGuid(), type, result);

        comparison.BenchmarkType.Should().Be(type);
    }
}
