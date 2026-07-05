namespace Certus.Application.Trading.DTOs;

public record PerformanceSnapshotDto(
    DateOnly Date,
    decimal ActualReturn,
    decimal SupposedReturn,
    decimal DailyPnl,
    decimal Equity,
    decimal Drawdown,
    decimal Sharpe);