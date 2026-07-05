using Certus.Domain.SharedKernel;

namespace Certus.Domain.Evaluation.ValueObjects;

public record BenchmarkResult 
{
    public decimal BenchmarkReturn { get; }
    public decimal Alpha { get; }
    public decimal Beta { get; }
    public decimal InformationRatio { get; }
    public decimal TrackingError { get; }

    public BenchmarkResult(
        decimal benchmarkReturn,
        decimal alpha,
        decimal beta,
        decimal informationRatio,
        decimal trackingError)
    {
        BenchmarkReturn = benchmarkReturn;
        Alpha = alpha;
        Beta = beta;
        InformationRatio = informationRatio;
        TrackingError = trackingError;
    }

    public bool OutperformsBenchmark => Alpha > 0;
}
