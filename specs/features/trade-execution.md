# Trade Execution

## Status
Active

## Overview
Trade Execution manages the lifecycle of individual trades from signal to close, including order management, fill tracking, PnL calculation, and slippage monitoring.

## Requirements

### Functional Requirements
- [ ] FR-001: Create a trade from a signal with strategy, portfolio, symbol, side, entry price, and quantity
- [ ] FR-002: Open a pending trade with an entry order
- [ ] FR-003: Close an open trade with an exit order, calculating PnL
- [ ] FR-004: Cancel a pending or open trade with a reason
- [ ] FR-005: Track orders (submit, fill, cancel, reject) within a trade
- [ ] FR-006: Record execution logs for audit trail
- [ ] FR-007: Calculate slippage for each order

### Non-Functional Requirements
- [ ] NFR-001: PnL must be calculated correctly for both Long and Short trades
- [ ] NFR-002: Domain events must be raised for trade lifecycle changes

## Data Model

### Trade (Aggregate Root)
```
Trade
  Id               : Guid              // Unique identifier
  StrategyId       : Guid              // Parent strategy
  PortfolioId      : Guid              // Parent portfolio
  Symbol           : Symbol            // Trading pair
  Side             : TradeSide         // Long | Short
  EntryTime        : DateTime          // When trade was created
  ExitTime         : DateTime?         // When trade was closed (null if open)
  EntryPrice       : decimal           // Price at entry
  ExitPrice        : decimal?          // Price at exit (null if open)
  Quantity         : decimal           // Position size
  PnL              : decimal           // Realized profit/loss
  PnLPercent       : decimal           // PnL as percentage
  Fees             : decimal           // Total fees paid
  Slippage         : decimal           // Total execution slippage
  Status           : TradeStatus       // Pending | Open | Closed | Cancelled
  AgentReason      : string            // AI decision explanation
  Lifecycle        : TradeLifecycle    // Timestamps through lifecycle
  Orders           : Order[]           // Child orders
  ExecutionLogs    : ExecutionLog[]    // Audit trail
```

### Order (Entity)
```
Order
  Id               : Guid
  TradeId          : Guid
  Symbol           : string
  Side             : TradeSide
  OrderType        : OrderType         // Market | Limit | StopLoss | StopLimit | TrailingStop
  Status           : OrderStatus       // Pending | Submitted | PartiallyFilled | Filled | Cancelled | Rejected
  RequestedQuantity : decimal
  FilledQuantity   : decimal
  AverageFillPrice : decimal
  LimitPrice       : decimal?
  TotalFees        : decimal
  Slippage         : decimal
  CreatedAt        : DateTime
  Fills            : Fill[]
```

### Fill (Entity)
```
Fill
  Id               : Guid
  OrderId          : Guid
  Price            : decimal
  Quantity         : decimal
  Fees             : decimal
  Commission       : decimal
  Swap             : decimal
  Timestamp        : DateTime
```

### ExecutionLog (Entity)
```
ExecutionLog
  Id               : Guid
  TradeId          : Guid
  Action           : string
  Details          : string
  LoggedAt         : DateTime
```

## Business Rules

1. **BR-001**: Trade must be Pending to Open
2. **BR-002**: Trade must be Open to Close
3. **BR-003**: Closed trade cannot be Cancelled
4. **BR-004**: PnL for Long = (ExitPrice - EntryPrice) * Quantity
5. **BR-005**: PnL for Short = (EntryPrice - ExitPrice) * Quantity
6. **BR-006**: PnLPercent = PnL / (EntryPrice * Quantity) * 100
7. **BR-007**: ExitTime must be after EntryTime
8. **BR-008**: Fees and Slippage accumulate across orders

## Acceptance Criteria

- [ ] AC-001: Given a pending trade, when opening with an entry order, then status becomes Open and entry price is set
- [ ] AC-002: Given an open Long trade, when closing with exit price higher than entry, then PnL is positive
- [ ] AC-003: Given an open Short trade, when closing with exit price lower than entry, then PnL is positive
- [ ] AC-004: Given a closed trade, when attempting to cancel, then InvalidOperationException is thrown
- [ ] AC-005: Given a trade with multiple orders, when checking fees, then fees accumulate across all orders
- [ ] AC-006: Given a trade, when checking Duration, then it returns null if open, or time difference if closed
- [ ] AC-007: Given a trade with positive PnL, when checking IsWinning, then it returns true
- [ ] AC-008: Given a trade with negative PnL, when checking IsWinning, then it returns false

## Test Mapping

| Acceptance Criteria | Test File | Test Method |
|---------------------|-----------|-------------|
| AC-001 | TradeTests.cs | Open_Should_Set_Status_To_Open |
| AC-002 | TradeTests.cs | Close_Long_Should_Calculate_Positive_PnL |
| AC-003 | TradeTests.cs | Close_Short_Should_Calculate_Positive_PnL |
| AC-004 | TradeTests.cs | Cancel_Closed_Should_Throw |
| AC-005 | TradeTests.cs | Fees_Should_Accumulate_Across_Orders |
| AC-006 | TradeTests.cs | Duration_Should_Be_Null_When_Open |
| AC-007 | TradeTests.cs | IsWinning_Should_Be_True_When_PnL_Positive |
| AC-008 | TradeTests.cs | IsWinning_Should_Be_False_When_PnL_Negative |

## Dependencies

- Domain: SharedKernel (AggregateRoot, Entity, Symbol, IDomainEvent)
- Domain: Execution.Enums (TradeSide, TradeStatus, OrderType, OrderStatus)
- Domain: Execution.Entities (Order, Fill, ExecutionLog)
- Domain: Execution.ValueObjects (TradeLifecycle, ExecutionResult, OrderRequest, SlippageMetrics, TradeIdentifier)
