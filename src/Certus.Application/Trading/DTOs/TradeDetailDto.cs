namespace Certus.Application.Trading.DTOs;

public record TradeDetailDto(
    TradeDto Trade,
    string AgentReason,
    decimal PortfolioImpact,
    decimal StrategyImpact);