using Certus.Domain.SharedKernel;

namespace Certus.Domain.Execution.ValueObjects;

public record TradeIdentifier 
{
    public Guid StrategyId { get; }
    public Guid PortfolioId { get; }
    public Symbol Symbol { get; }
    public DateTime EntryTime { get; }

    public TradeIdentifier(
        Guid strategyId,
        Guid portfolioId,
        Symbol symbol,
        DateTime entryTime)
    {
        StrategyId = strategyId;
        PortfolioId = portfolioId;
        Symbol = symbol;
        EntryTime = entryTime;
    }
}
