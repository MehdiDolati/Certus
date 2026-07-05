using Certus.Domain.RiskAndPortfolio.Enums;

namespace Certus.Application.Portfolios.DTOs;

public record PortfolioDto(
    Guid Id,
    string Name,
    string Description,
    PortfolioStatus Status,
    decimal ActualReturn,
    decimal SupposedReturn,
    decimal Variance,
    decimal Sharpe,
    decimal MaxDrawdown,
    int TradeCount,
    decimal WinRate,
    decimal Aum);
