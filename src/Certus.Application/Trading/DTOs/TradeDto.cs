using Certus.Domain.Execution.Enums;

namespace Certus.Application.Trading.DTOs;

public record TradeDto(
    Guid Id,
    string StrategyName,
    string Symbol,
    TradeSide Side,
    DateTime EntryTime,
    DateTime? ExitTime,
    decimal EntryPrice,
    decimal? ExitPrice,
    decimal Quantity,
    decimal PnL,
    decimal PnLPercent,
    TimeSpan? Duration,
    decimal Fees,
    decimal Slippage,
    TradeStatus Status);
