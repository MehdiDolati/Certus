# Platform Integration

## Status
Draft

## Overview
Platform integration enables Certus to connect to external trading platforms (MetaTrader 4, MetaTrader 5, online brokers) and import portfolio, strategy, and trade data via a plugin architecture. Certus is platform-agnostic — it reads data from shared files produced by platform-specific Expert Advisors or adapters, without modifying those files.

## Requirements

### Functional Requirements
- [ ] FR-001: Connect to a trading platform via a plugin adapter (file-based or API-based)
- [ ] FR-002: Watch shared files for real-time data updates using FileSystemWatcher
- [ ] FR-003: Import a portfolio from a connected platform, creating a Certus portfolio with platform reference
- [ ] FR-004: Read strategy status from platform data — if strategies are sending logs, they are live
- [ ] FR-005: Import individual trade records from platform with full lifecycle data (entry, exit, PnL, fees, slippage)
- [ ] FR-006: Detect duplicate trades by external ID and skip re-import
- [ ] FR-007: Calculate deviations from expected behavior by comparing actual vs expected trade outcomes
- [ ] FR-008: Display platform connection status on the dashboard
- [ ] FR-009: Manually insert a portfolio without platform connection (status starts as non-active)
- [ ] FR-010: Plugin architecture — new platforms added by implementing IPlatformPlugin interface

### Non-Functional Requirements
- [ ] NFR-001: File reads must handle file locking gracefully (retry with backoff, read-only access)
- [ ] NFR-002: File change detection latency < 200ms
- [ ] NFR-003: Plugin loading must not crash the application if a plugin fails to load
- [ ] NFR-004: All file operations are read-only — Certus never writes to platform files

## Data Model

### PlatformConnection (Aggregate Root)
```
PlatformConnection
  Id                   : Guid                    // Unique identifier
  PlatformId           : string                  // Plugin identifier (e.g., "metatrader4")
  PlatformName         : string                  // Display name (e.g., "MetaTrader 4")
  Status               : PlatformConnectionStatus // Disconnected | Connecting | Connected | Error | Reconnecting
  ConnectedAt          : DateTime?               // When connection was established
  DisconnectedAt       : DateTime?               // When connection was lost
  LastDataReceivedAt   : DateTime?               // Last successful data read
  Config               : PlatformConfig          // Connection configuration (owned)
  ErrorMessage         : string?                 // Last error message
  RetryCount           : int                     // Number of consecutive errors
```

### PlatformPortfolio (Value Object)
```
PlatformPortfolio
  ExternalId           : string                  // Platform's portfolio ID
  Name                 : string                  // Portfolio name on platform
  Balance              : decimal                 // Account balance
  Equity               : decimal                 // Current equity
  Margin               : decimal                 // Used margin
  FreeMargin           : decimal                 // Available margin
  Profit               : decimal                 // Floating P&L
  Timestamp            : DateTime                // When this snapshot was taken
  Strategies           : PlatformStrategy[]      // Strategies in this portfolio
```

### PlatformStrategy (Value Object)
```
PlatformStrategy
  ExternalId           : string                  // Platform's strategy/EA identifier
  Name                 : string                  // Strategy display name
  IsActive             : bool                    // Whether strategy is currently running
  Profit               : decimal                 // Strategy profit
  TotalTrades          : int                     // Total trades executed
  WinningTrades        : int                     // Winning trade count
  LosingTrades         : int                     // Losing trade count
  LastTradeTime        : DateTime?               // When last trade was executed
```

### PlatformTrade (Value Object)
```
PlatformTrade
  ExternalId           : string                  // Platform's trade ticket number
  StrategyExternalId   : string                  // Parent strategy identifier
  Symbol               : string                  // Trading instrument
  Side                 : TradeSide               // Buy | Sell
  Volume               : decimal                 // Position size
  OpenPrice            : decimal                 // Entry price
  ClosePrice           : decimal?                // Exit price (null if open)
  StopLoss             : decimal                 // Stop loss level
  TakeProfit           : decimal                 // Take profit level
  Profit               : decimal                 // Realized profit
  Commission           : decimal                 // Broker commission
  Swap                 : decimal                 // Overnight swap costs
  OpenTime             : DateTime                // When trade was opened
  CloseTime            : DateTime?               // When trade was closed (null if open)
  Comment              : string                  // Trade comment/magic number
```

### ImportedTrade (Entity)
```
ImportedTrade
  Id                   : Guid                    // Certus internal ID
  ExternalId           : string                  // Platform's trade ID (unique)
  StrategyId           : Guid                    // Certus strategy reference
  PortfolioId          : Guid                    // Certus portfolio reference
  ConnectionId         : Guid                    // Platform connection reference
  Symbol               : string                  // Trading instrument
  Side                 : string                  // Buy | Sell
  Volume               : decimal                 // Position size
  OpenPrice            : decimal                 // Entry price
  ClosePrice           : decimal?                // Exit price
  StopLoss             : decimal                 // Stop loss
  TakeProfit           : decimal                 // Take profit
  Profit               : decimal                 // Realized profit
  Commission           : decimal                 // Commission
  Swap                 : decimal                 // Swap
  PnL                  : decimal                 // Computed: Profit + Commission + Swap
  OpenTime             : DateTime                // Entry time
  CloseTime            : DateTime?               // Exit time
  Comment              : string                  // Trade comment
  ImportedAt           : DateTime                // When imported into Certus
```

## Business Rules

1. **BR-001**: Certus must never write to or modify platform files — all file operations are read-only
2. **BR-002**: File reads must retry up to 3 times with exponential backoff when file is locked by the platform
3. **BR-003**: Duplicate trades (same ExternalId) are skipped during import
4. **BR-004**: A portfolio imported from a platform has IsPlatformManaged = true
5. **BR-005**: Platform connection status transitions: Disconnected → Connecting → Connected, Connected → Error → Reconnecting → Connected
6. **BR-006**: After 5 consecutive errors, the connection stops retrying
7. **BR-007**: All strategies in a portfolio must be sending data for the portfolio to be considered "running"
8. **BR-008**: A manually created portfolio starts with status that is NOT Active
9. **BR-009**: Plugins are loaded from a directory at startup; failed plugins are logged but don't crash the app

## Acceptance Criteria

### Platform Connection
- [ ] AC-001: Given a valid MT4 plugin and file path, when connecting, then a PlatformConnection is created with status Connected
- [ ] AC-002: Given no plugin for a platform ID, when connecting, then an error is thrown
- [ ] AC-003: Given a connected platform, when disconnecting, then status becomes Disconnected and file watching stops
- [ ] AC-004: Given a connection with 5 consecutive errors, when checking ShouldReconnect, then it returns false

### Portfolio Import
- [ ] AC-005: Given a connected platform with portfolio data, when importing, then a Certus portfolio is created with IsPlatformManaged = true
- [ ] AC-006: Given a connected platform with no data for a portfolio ID, when importing, then null is returned
- [ ] AC-007: Given a portfolio with empty name, when manually creating, then validation error is thrown
- [ ] AC-008: Given a manually created portfolio, when checking status, then it is NOT Active

### Strategy Status
- [ ] AC-009: Given platform data showing an active strategy, when reading status, then IsActive is true
- [ ] AC-010: Given platform data showing an inactive strategy, when reading status, then IsActive is false

### Trade Import
- [ ] AC-011: Given a connected platform with trade data, when importing, then trades are saved with ImportedTrade records
- [ ] AC-012: Given a trade with ExternalId that already exists, when importing, then the trade is skipped
- [ ] AC-013: Given an imported trade, when computing PnL, then PnL = Profit + Commission + Swap

### File Handling
- [ ] AC-014: Given a locked file, when reading, then the service retries up to 3 times with backoff
- [ ] AC-015: Given a file that doesn't exist, when reading, then null is returned
- [ ] AC-016: Given a FileSystemWatcher watching a file, when the file changes, then a FileChanged event is raised

### Plugin Architecture
- [ ] AC-017: Given a plugin directory with DLLs implementing IPlatformPlugin, when loading plugins, then all valid plugins are registered
- [ ] AC-018: Given a DLL that doesn't implement IPlatformPlugin, when loading, then it is skipped without error

### Strategy Information Collection
- [ ] AC-019: Given an EA writing strategy data, when Certus reads portfolio_status.json, then strategy parameters, open positions, and performance metrics are captured
- [ ] AC-020: Given imported trades and expected trade outcomes, when calculating deviations, then deviation percentage and direction are computed
- [ ] AC-021: Given the EA writing portfolio_status.json, when Certus reads it, then the JSON matches the defined schema (timestamp, portfolio, strategies array)
- [ ] AC-022: Given the EA writing trades.json, when Certus reads it, then the JSON matches the defined schema (timestamp, trades array with full lifecycle data)
- [ ] AC-023: Given the EA configured with an update interval, when the interval elapses, then portfolio_status.json is rewritten with current data
- [ ] AC-024: Given a trade occurring on the platform, when the EA detects it, then trades.json is appended immediately

## Test Mapping

| Acceptance Criteria | Test File | Test Method |
|---------------------|-----------|-------------|
| AC-001 | PlatformConnectionTests.cs | Connect_Should_Set_Status_To_Connected |
| AC-002 | PlatformServiceTests.cs | ConnectAsync_Should_Throw_When_No_Plugin_Found |
| AC-003 | PlatformConnectionTests.cs | Disconnect_Should_Set_Status_To_Disconnected |
| AC-004 | PlatformConnectionTests.cs | ShouldReconnect_Should_Return_False_When_RetryCount_Exceeds_Max |
| AC-005 | PlatformServiceTests.cs | ImportPortfolioAsync_Should_Create_Portfolio_From_Platform_Data |
| AC-006 | PlatformServiceTests.cs | ImportPortfolioAsync_Should_Return_Null_When_No_Data |
| AC-007 | PortfolioTests.cs | CreateAsync_Should_Throw_On_Empty_Name |
| AC-008 | PortfolioTests.cs | Manually_Created_Portfolio_Should_Not_Be_Active |
| AC-009 | PlatformStrategyTests.cs | PlatformStrategy_IsActive_Should_Be_True_When_Running |
| AC-010 | PlatformStrategyTests.cs | PlatformStrategy_IsActive_Should_Be_False_When_Not_Running |
| AC-011 | PlatformServiceTests.cs | ImportTradesAsync_Should_Import_New_Trades |
| AC-012 | PlatformServiceTests.cs | ImportTradesAsync_Should_Skip_Existing_Trades |
| AC-013 | PlatformTradeTests.cs | PlatformTrade_PnL_Should_Be_Profit_Plus_Commission_Plus_Swap |
| AC-014 | FileImportServiceTests.cs | ReadFileAsync_Should_Retry_When_File_Locked |
| AC-015 | FileImportServiceTests.cs | ReadFileAsync_Should_Return_Null_When_File_Not_Found |
| AC-016 | FileImportServiceTests.cs | FileSystemWatcher_Should_Raise_Event_On_Change |
| AC-017 | PluginLoaderTests.cs | LoadPlugins_Should_Register_Valid_Plugins |
| AC-018 | PluginLoaderTests.cs | LoadPlugins_Should_Skip_Invalid_Assemblies |
| AC-019 | Mt4AdapterTests.cs | GetPortfoliosAsync_Should_Collect_Strategy_Data |
| AC-020 | DeviationCalculationTests.cs | CalculateDeviation_Should_Compute_Percentage_And_Direction |
| AC-021 | Mt4JsonParserTests.cs | ParsePortfolio_Should_Match_Defining_Schema |
| AC-022 | Mt4JsonParserTests.cs | ParseTrades_Should_Match_Defining_Schema |
| AC-023 | Mt4AdapterTests.cs | Portfolio_Status_Should_Be_Updated_Per_Interval |
| AC-024 | Mt4AdapterTests.cs | Trade_Should_Be_Appended_Immediately_On_Detection |

## API Contract

```csharp
public interface IPlatformService
{
    Task<PlatformConnectionDto> ConnectAsync(ConnectPlatformRequest request);
    Task DisconnectAsync(Guid connectionId);
    Task<List<PlatformConnectionDto>> GetConnectionsAsync();
    Task<PlatformConnectionDto?> GetConnectionAsync(Guid connectionId);
    Task<PlatformStatusDto> GetStatusAsync(Guid connectionId);
    Task<PlatformPortfolioDto?> ImportPortfolioAsync(Guid connectionId, string externalPortfolioId, string? portfolioName = null);
    Task<List<PlatformStrategyDto>> GetStrategiesAsync(Guid connectionId, string portfolioExternalId);
    Task<ImportTradesResult> ImportTradesAsync(Guid connectionId, string strategyExternalId, DateTime? from = null, DateTime? to = null);
    Task<PlatformDashboardDto> GetDashboardAsync();
}
```

## Dependencies

- Domain: `PlatformConnection`, `PlatformPortfolio`, `PlatformStrategy`, `PlatformTrade`, `ImportedTrade`
- Domain: `Portfolio` (updated with platform reference fields)
- Domain: `IPlatformAdapter`, `IPlatformPlugin`, `IPlatformDataParser`, `IFileImportService`
- Infrastructure: `PluginLoader`, `FileImportService`, `Mt4Plugin`, `Mt4Adapter`
- Infrastructure: `PlatformConnectionRepository`, `ImportedTradeRepository`
