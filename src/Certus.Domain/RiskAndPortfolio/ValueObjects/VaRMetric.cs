using Certus.Domain.SharedKernel;

namespace Certus.Domain.RiskAndPortfolio.ValueObjects;

public record VaRMetric 
{
    public decimal Value { get; }
    public decimal ConfidenceLevel { get; }
    public int TimeHorizonDays { get; }

    public VaRMetric(decimal value, decimal confidenceLevel, int timeHorizonDays)
    {
        Value = value;
        ConfidenceLevel = confidenceLevel;
        TimeHorizonDays = timeHorizonDays;
    }

    public decimal DailyVaR => TimeHorizonDays > 0 ? Value / (decimal)Math.Sqrt(TimeHorizonDays) : Value;
}
