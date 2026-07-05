using Certus.Domain.Evaluation.Enums;
using Certus.Domain.Evaluation.ValueObjects;
using Certus.Domain.SharedKernel;

namespace Certus.Domain.Evaluation.Entities;

public class BenchmarkComparison : Entity
{
    public Guid ReportId { get; private set; }
    public BenchmarkType BenchmarkType { get; private set; }
    public BenchmarkResult Result { get; private set; }

    private BenchmarkComparison() { }

    public BenchmarkComparison(
        Guid id,
        Guid reportId,
        BenchmarkType benchmarkType,
        BenchmarkResult result) : base(id)
    {
        ReportId = reportId;
        BenchmarkType = benchmarkType;
        Result = result;
    }
}
