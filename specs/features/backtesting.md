# Backtesting

## Status
Active

## Overview
Backtesting enables strategy simulation against historical data, tracking individual trades and computing performance metrics.

## Data Model

### BacktestRun (Entity)
- StrategyId: Guid
- Config: BacktestConfig (start/end dates, capital, symbols)
- Metrics: BacktestMetrics (results)
- Status: BacktestStatus (Pending/Running/Completed/Failed)
- Trades: BacktestTrade[]

**Methods**: Start, Complete(metrics), Fail(errorMessage)

### BacktestTrade (Entity)
- BacktestRunId: Guid
- Symbol, Side (Long/Short), EntryPrice, ExitPrice, PnL
- EntryTime, ExitTime

### StrategyParameter (Entity)
- Name: string
- Type: string (int, decimal, bool, string)
- Default, Min, Max: decimal
- CurrentValue: decimal

### Value Objects

| Value Object | Key Fields |
|--------------|------------|
| BacktestConfig | StartDate, EndDate, InitialCapital, Symbols[], TimeFrame |
| BacktestMetrics | TotalReturn, SharpeRatio, MaxDrawdown, WinRate, ProfitFactor, CalmarRatio, SortinoRatio, TotalTrades, AvgTradeDuration |

### Enums
- BacktestStatus: Pending, Running, Completed, Failed

## Acceptance Criteria

- [ ] AC-001: Given a BacktestRun, when starting, then Status becomes Running
- [ ] AC-002: Given a running BacktestRun, when completing with metrics, then Status becomes Completed and Metrics are set
- [ ] AC-003: Given a BacktestRun, when failing with error, then Status becomes Failed and ErrorMessage is set
