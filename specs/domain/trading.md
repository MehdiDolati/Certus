# Trade & PerformanceSnapshot Entities

## Status
Active

## Overview
Trades represent individual buy/sell operations executed by strategies. PerformanceSnapshots capture daily portfolio/strategy performance metrics.

## Data Model

### Trade

```
Trade
  Id            : Guid          // Unique identifier
  StrategyId    : Guid          // Parent strategy
  PortfolioId   : Guid          // Parent portfolio
  Symbol        : string        // Trading pair (e.g., "BTC/USD", max 20 chars)
  Side          : TradeSide     // Long | Short
  EntryTime     : DateTime      // When trade was opened
  ExitTime      : DateTime?     // When trade was closed (null if open)
  EntryPrice    : decimal       // Price at entry
  ExitPrice     : decimal?      // Price at exit (null if open)
  Quantity      : decimal       // Position size (max 8 decimal places)
  PnL           : decimal       // Realized profit/loss in USD
  PnLPercent    : decimal       // Realized PnL as percentage
  Fees          : decimal       // Total fees paid
  Slippage      : decimal       // Execution slippage
  Status        : TradeStatus   // Open | Closed | Cancelled
  AgentReason   : string        // AI decision explanation (max 2000 chars)
```

### PerformanceSnapshot

```
PerformanceSnapshot
  Id              : Guid          // Unique identifier
  PortfolioId     : Guid          // Parent portfolio
  StrategyId      : Guid?         // Null = portfolio-level rollup
  Date            : DateOnly      // Snapshot date
  ActualReturn    : decimal       // Cumulative return %
  SupposedReturn  : decimal       // Target/benchmark return %
  DailyPnL        : decimal       // Daily profit/loss
  Equity          : decimal       // Portfolio NAV
  Drawdown        : decimal       // Current drawdown %
  Sharpe          : decimal       // Rolling Sharpe ratio
```

## Enum Values

### TradeSide
| Value | Description |
|-------|-------------|
| Long | Buy position (profit when price rises) |
| Short | Sell position (profit when price falls) |

### TradeStatus
| Value | Description |
|-------|-------------|
| Open | Position is active |
| Closed | Position has been closed |
| Cancelled | Trade was cancelled before execution |

## Business Rules

1. **BR-001**: ExitTime must be after EntryTime when present
2. **BR-002**: ExitPrice must be present if and only if Status is Closed
3. **BR-003**: PnL for Long trades = (ExitPrice - EntryPrice) * Quantity
4. **BR-004**: PnL for Short trades = (EntryPrice - ExitPrice) * Quantity
5. **BR-005**: PnLPercent = PnL / (EntryPrice * Quantity) * 100

## Computed Properties (Trade)

| Property | Formula | Description |
|----------|---------|-------------|
| IsOpen | Status == Open | Whether position is active |
| IsClosed | Status == Closed | Whether position is closed |
| Duration | ExitTime - EntryTime | Time in trade (null if open) |
| IsWinning | PnL > 0 | Whether trade is profitable |

## Acceptance Criteria

- [ ] AC-001: Given a trade with Status Open, when checking IsOpen, then it returns true
- [ ] AC-002: Given a trade with Status Closed, when checking IsClosed, then it returns true
- [ ] AC-003: Given a trade with no ExitTime, when accessing Duration, then it returns null
- [ ] AC-004: Given a trade with EntryTime Jan 1 and ExitTime Jan 3 04:00, when accessing Duration, then it returns 2 days 4 hours
- [ ] AC-005: Given a trade with PnL of 3200, when checking IsWinning, then it returns true
- [ ] AC-006: Given a trade with PnL of -500, when checking IsWinning, then it returns false

## Test Mapping

| Acceptance Criteria | Test File | Test Method |
|---------------------|-----------|-------------|
| AC-001 | TradeTests.cs | Trade_IsOpen_Should_Return_True_When_Status_Is_Open |
| AC-002 | TradeTests.cs | Trade_IsClosed_Should_Return_True_When_Status_Is_Closed |
| AC-003 | TradeTests.cs | Trade_Duration_Should_Return_Null_When_No_ExitTime |
| AC-004 | TradeTests.cs | Trade_Duration_Should_Return_Difference_When_ExitTime_Exists |
| AC-005 | TradeTests.cs | Trade_IsWinning_Should_Return_True_When_PnL_Is_Positive |
| AC-006 | TradeTests.cs | Trade_IsWinning_Should_Return_False_When_PnL_Is_Negative |
