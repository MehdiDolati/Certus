using Certus.Domain.Strategy.Enums;

namespace Certus.Application.Strategies.DTOs;

public record StrategyPerformanceDto(
    Guid Id,
    string Name,
    string Type,
    decimal ContributionPercent,
    decimal ActualReturn,
    decimal SupposedReturn,
    decimal Variance,
    decimal Sharpe,
    int TradeCount,
    decimal WinRate,
    StrategyStatus Status);
