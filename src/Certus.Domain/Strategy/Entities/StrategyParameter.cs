using Certus.Domain.SharedKernel;

namespace Certus.Domain.Strategy.Entities;

public class StrategyParameter : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty;
    public decimal DefaultValue { get; private set; }
    public decimal MinValue { get; private set; }
    public decimal MaxValue { get; private set; }
    public decimal CurrentValue { get; private set; }

    private StrategyParameter() { }

    public StrategyParameter(
        Guid id,
        string name,
        string type,
        decimal defaultValue,
        decimal minValue,
        decimal maxValue) : base(id)
    {
        Name = name;
        Type = type;
        DefaultValue = defaultValue;
        MinValue = minValue;
        MaxValue = maxValue;
        CurrentValue = defaultValue;
    }

    public void UpdateValue(decimal newValue)
    {
        if (newValue < MinValue || newValue > MaxValue)
            throw new ArgumentOutOfRangeException(nameof(newValue),
                $"Value must be between {MinValue} and {MaxValue}");

        CurrentValue = newValue;
    }

    public void ResetToDefault()
    {
        CurrentValue = DefaultValue;
    }
}
