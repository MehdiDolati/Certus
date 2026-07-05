using Certus.Domain.SharedKernel;

namespace Certus.Domain.RiskAndPortfolio.ValueObjects;

public record DrawdownLimit 
{
    public decimal MaxDrawdown { get; }
    public decimal WarningLevel { get; }
    public decimal HaltLevel { get; }

    public DrawdownLimit(decimal maxDrawdown, decimal warningLevel, decimal haltLevel)
    {
        if (haltLevel > warningLevel || warningLevel > maxDrawdown)
            throw new ArgumentException("Levels must be ordered: halt < warning < max");

        MaxDrawdown = maxDrawdown;
        WarningLevel = warningLevel;
        HaltLevel = haltLevel;
    }

    public bool ShouldHalt(decimal currentDrawdown) => currentDrawdown >= HaltLevel;
    public bool ShouldWarn(decimal currentDrawdown) => currentDrawdown >= WarningLevel;
}
