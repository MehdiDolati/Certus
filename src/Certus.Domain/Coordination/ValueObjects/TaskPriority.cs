using Certus.Domain.SharedKernel;

namespace Certus.Domain.Coordination.ValueObjects;

public record TaskPriority 
{
    public int Value { get; }
    public bool IsUrgent { get; }

    public TaskPriority(int value, bool isUrgent = false)
    {
        if (value < 1 || value > 10)
            throw new ArgumentOutOfRangeException(nameof(value), "Priority must be between 1 and 10");

        Value = value;
        IsUrgent = isUrgent;
    }

    public static TaskPriority Low => new(3);
    public static TaskPriority Normal => new(5);
    public static TaskPriority High => new(8);
    public static TaskPriority Critical => new(10, true);
}
