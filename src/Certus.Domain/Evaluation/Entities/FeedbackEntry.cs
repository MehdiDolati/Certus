using Certus.Domain.Evaluation.Enums;
using Certus.Domain.SharedKernel;

namespace Certus.Domain.Evaluation.Entities;

public class FeedbackEntry : Entity
{
    public Guid ReportId { get; private set; }
    public FeedbackType Type { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public Guid? StrategyId { get; private set; }
    public DateTime GeneratedAt { get; private set; }

    private FeedbackEntry() { }

    public FeedbackEntry(
        Guid id,
        Guid reportId,
        FeedbackType type,
        string message,
        Guid? strategyId = null) : base(id)
    {
        ReportId = reportId;
        Type = type;
        Message = message;
        StrategyId = strategyId;
        GeneratedAt = DateTime.UtcNow;
    }
}
