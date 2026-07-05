using Certus.Domain.SharedKernel;

namespace Certus.Domain.Execution.Entities;

public class ExecutionLog : Entity
{
    public Guid TradeId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string Details { get; private set; } = string.Empty;
    public DateTime LoggedAt { get; private set; }

    private ExecutionLog() { }

    public ExecutionLog(Guid id, Guid tradeId, string action, string details) : base(id)
    {
        TradeId = tradeId;
        Action = action;
        Details = details;
        LoggedAt = DateTime.UtcNow;
    }
}
