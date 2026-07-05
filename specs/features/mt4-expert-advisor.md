# MetaTrader 4 Expert Advisor

## Status
Active

## Overview
The Certus EA is a **single MetaTrader 4 Expert Advisor** that runs on any one chart and monitors ALL trading activity across the entire account. It reads all orders via `OrderSelect` (MODE_TRADES and MODE_HISTORY), groups them by magic number to identify strategies, and writes the data to shared JSON files that Certus reads via FileSystemWatcher.

**Key design decision:** The EA does NOT need to be attached to every chart. One instance on any chart monitors everything — EAs, manual trades, all symbols and timeframes.

## Requirements

### Functional Requirements
- [ ] FR-001: Single EA instance on any chart monitors ALL account activity
- [ ] FR-002: Write portfolio status (balance, equity, margin, profit) to `portfolio_status.json` at configurable intervals
- [ ] FR-003: Group orders by magic number to identify different strategies/EAs
- [ ] FR-004: Log individual trades to `trades.json` immediately when they occur (open, close, modify)
- [ ] FR-005: Handle file locking gracefully — use temporary files and atomic rename
- [ ] FR-006: Track both EAs and manual trading (magic number 0 = manual)

### Non-Functional Requirements
- [ ] NFR-001: Portfolio status update interval is configurable (default: 5 seconds)
- [ ] NFR-002: Trade logging must not miss trades (write on OrderSend, OrderClose, OrderModify)
- [ ] NFR-003: File writes must be atomic (write to temp file, then rename)
- [ ] NFR-004: EA must handle MT4 terminal restarts gracefully

## File Formats

### portfolio_status.json

Written periodically (every N seconds). Overwrites previous content.

```json
{
  "timestamp": "2026-07-05T10:30:00Z",
  "portfolio": {
    "id": "12345",
    "name": "Mehdi Dolati",
    "balance": 100000.00,
    "equity": 102500.00,
    "margin": 15000.00,
    "freeMargin": 87500.00,
    "profit": 2500.00
  },
  "strategies": [
    {
      "id": "EA_Momentum_01",
      "name": "Momentum Strategy",
      "active": true,
      "profit": 1800.00,
      "totalTrades": 45,
      "lastTradeTime": "2026-07-05T10:25:00Z"
    }
  ]
}
```

### trades.json

Append-only log. Each trade event adds a new entry.

```json
{
  "timestamp": "2026-07-05T10:30:00Z",
  "trades": [
    {
      "id": "789012",
      "strategyId": "EA_Momentum_01",
      "symbol": "EURUSD",
      "side": "buy",
      "volume": 0.10,
      "openPrice": 1.0850,
      "closePrice": 1.0875,
      "stopLoss": 1.0830,
      "takeProfit": 1.0900,
      "profit": 25.00,
      "commission": -1.50,
      "swap": -0.25,
      "openTime": "2026-07-05T09:15:00Z",
      "closeTime": "2026-07-05T10:25:00Z",
      "comment": "Momentum signal"
    }
  ]
}
```

### Trade Event Types

| Event | Trigger | When |
|-------|---------|------|
| `open` | OrderSend successful | Position opened |
| `close` | OrderClose successful | Position closed |
| `modify` | OrderModify successful | SL/TP changed |
| `pending` | OrderSend for pending order | Pending order placed |
| `cancel` | OrderDelete successful | Pending order cancelled |

## EA Architecture

```
MetaTrader4/Experts/CertusEA.mq4
├── CertusEA.mq4                    # Main EA entry point (OnInit, OnTick, OnDeinit)
├── Include/
│   ├── CertusConfig.mqh           # Input parameters and constants
│   ├── CertusFileWriter.mqh       # Atomic file write utilities
│   ├── CertusPortfolioStatus.mqh  # Portfolio status JSON builder
│   ├── CertusTradeLogger.mqh      # Trade event logging
│   └── CertusStrategyInfo.mqh     # Strategy data collection
└── Files/                          # MT4's data folder
    └── Certus/
        ├── portfolio_status.json   # Current portfolio + strategy status
        └── trades.json             # Trade event log (append-only)
```

### Input Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| OutputDirectory | string | "Certus" | Subdirectory under MT4 Files/ |
| UpdateIntervalSeconds | int | 5 | How often to write portfolio_status.json |
| LogAllTrades | bool | true | Log trades on open/close/modify |
| LogLevel | string | "INFO" | INFO, DEBUG, WARN, ERROR |

### Key Functions

```
OnInit()
  - Create output directory if not exists
  - Set timer for periodic updates
  - Initial portfolio status write

OnTimer()
  - Write portfolio_status.json with current data

OnTrade()
  - Log trade event to trades.json

OnTick()
  - (Optional) Update real-time price data in open positions

BuildPortfolioStatusJSON()
  - Collect AccountInfo (balance, equity, margin, etc.)
  - Enumerate running EAs via OnTick/OnTimer
  - Build JSON string

BuildTradesJSON()
  - Read order history since last check
  - Filter by magic number (strategy identifier)
  - Build JSON for new/modified/closed trades

WriteFileAtomic(filePath, content)
  - Write to filePath.tmp
  - Rename filePath.tmp to filePath (atomic on most filesystems)
```

## Business Rules

1. **BR-001**: The EA owns the files; Certus only reads them
2. **BR-002**: portfolio_status.json is overwritten each update cycle (latest state)
3. **BR-003**: trades.json is append-only (never overwritten)
4. **BR-004**: File writes must be atomic to prevent Certus reading partial JSON
5. **BR-005**: Magic number 0 = manual trading, non-zero = EA/strategy
6. **BR-006**: Single EA instance on any chart monitors entire account

## Acceptance Criteria

### Portfolio Status
- [ ] AC-001: Given the EA running on one chart, when writing portfolio_status.json, then it contains account data (balance, equity, margin, profit)
- [ ] AC-002: Given multiple EAs running on different charts, when writing portfolio_status.json, then all are listed as strategies grouped by magic number
- [ ] AC-003: Given manual trades (magic number 0), when writing portfolio_status.json, then they appear as "Manual Trading" strategy
- [ ] AC-004: Given a strategy with open orders, when checking active status, then it shows as active

### Trade Logging
- [ ] AC-005: Given a trade opened on any chart, when OnTradeTransaction fires, then trades.json contains an "open" event
- [ ] AC-006: Given a trade closed, when OnTradeTransaction fires, then trades.json contains a "close" event with close_price and profit
- [ ] AC-007: Given a trade modified (SL/TP changed), when OnTradeTransaction fires, then trades.json contains a "modify" event
- [ ] AC-008: Given trades.json already has content, when a new trade occurs, then the new trade is appended without losing existing data

### File Handling
- [ ] AC-009: Given Certus reading portfolio_status.json while EA is writing, then Certus reads a valid complete JSON (atomic write)
- [ ] AC-010: Given the EA writing to a non-existent directory, when writing, then the directory is created automatically

### Error Handling
- [ ] AC-011: Given a file write failure, when retrying, then the EA retries up to 3 times before logging an error
- [ ] AC-012: Given an EA removed from a chart, when OnDeinit runs, then no partial files are left behind

## Test Mapping

| Acceptance Criteria | Test File | Test Method |
|---------------------|-----------|-------------|
| AC-001 | Mt4PortfolioStatusTests.cs | UpdateInterval_Should_Trigger_Write |
| AC-002 | Mt4PortfolioStatusTests.cs | OpenPositions_Should_Be_Included |
| AC-003 | Mt4PortfolioStatusTests.cs | MultipleEAs_Should_All_Be_In_Strategies |
| AC-004 | Mt4PortfolioStatusTests.cs | Parameters_Should_Be_Captured |
| AC-005 | Mt4TradeLoggerTests.cs | OrderSend_Should_Log_Open_Event |
| AC-006 | Mt4TradeLoggerTests.cs | OrderClose_Should_Log_Close_Event |
| AC-007 | Mt4TradeLoggerTests.cs | OrderModify_Should_Log_Modify_Event |
| AC-008 | Mt4TradeLoggerTests.cs | Append_Should_Not_Overwrite_Existing |
| AC-009 | Mt4FileWriterTests.cs | AtomicWrite_Should_Be_Interrupt_Safe |
| AC-010 | Mt4FileWriterTests.cs | CreateDirectory_Should_Be_Automatic |
| AC-011 | Mt4EALifecycleTests.cs | Restart_Should_Resume_Writing |
| AC-012 | Mt4FileWriterTests.cs | Retry_Should_Up_To_3_Times |
| AC-013 | Mt4EALifecycleTests.cs | Deinit_Should_Clean_Up |

## Dependencies

- Certus side: `Mt4Adapter`, `Mt4JsonParser`, `FileImportService` (reads the files this EA writes)
- Platform integration spec: `specs/features/platform-integration.md`
