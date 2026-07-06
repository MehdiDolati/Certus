# Dashboard Real Data Integration

## Status
Active

## Overview
Wire all Dashboard UI components to real database data, replacing hardcoded values and empty placeholders with live queries. Includes a new Home overview page and Platform management page.

## Requirements

### Functional Requirements
- [ ] FR-001: Home page displays overview KPIs from DB: total portfolios, active portfolios, total AUM, total PnL, executed trades, imported trades, platform connections, avg Sharpe
- [ ] FR-002: Home page displays platform connection status cards with name, status, and last data timestamp
- [ ] FR-003: Home page provides quick action buttons to Portfolios and Platform pages
- [ ] FR-004: Home page shows system health summary (platform status, total PnL, imported trade count)
- [ ] FR-005: Platform page (`/platform`) shows connection list with status, connected-at, last-data-received, error messages, retry counts
- [ ] FR-006: Platform page shows aggregate stats: total connections, active connections, imported trades, total PnL
- [ ] FR-007: Platform page shows imported portfolio count
- [ ] FR-008: NavMenu includes Platform link (`/platform`)
- [ ] FR-009: TradeService resolves StrategyName from IStrategyDefinitionRepository instead of hardcoding empty string
- [ ] FR-010: TradeService computes PortfolioImpact (sum of closed trade PnL in portfolio) and StrategyImpact (sum of closed trade PnL in strategy)
- [ ] FR-011: PortfolioDetail respects period dropdown selection on initial load (not hardcoded 30 days)
- [ ] FR-012: PortfolioDetail period changes reload all data (summary, equity curve, drawdown, strategies, trades)

### Non-Functional Requirements
- [ ] NFR-001: Home page load < 2 seconds
- [ ] NFR-002: Platform page load < 1.5 seconds
- [ ] NFR-003: Period change on PortfolioDetail reloads within 1 second

## Data Model

### Input
- `PlatformDashboardDto` from `IPlatformService.GetDashboardAsync()`
- `PortfolioDto` list from `IPortfolioService.GetAllPortfoliosAsync()`
- `StrategyDefinition` from `IStrategyDefinitionRepository.GetByIdAsync()`

### Output
- `DashboardOverview` (Home page KPIs)
- `PlatformDashboardDto` (Platform page data)
- `TradeDto.StrategyName` (resolved from strategy)
- `TradeDetailDto.PortfolioImpact` / `StrategyImpact` (computed from trade data)

## Acceptance Criteria

- [ ] AC-001: Given portfolios in DB, when loading Home page, then KPI cards show real portfolio count, AUM, and PnL
- [ ] AC-002: Given platform connections in DB, when loading Home page, then connection status cards show real status
- [ ] AC-003: Given no platform connections, when loading Home page, then "No platform connections configured" message is shown
- [ ] AC-004: Given a connected platform, when loading Platform page, then connection shows "Connected" status with correct timestamps
- [ ] AC-005: Given trades with strategy references, when viewing trade log, then StrategyName column shows the strategy name (not empty)
- [ ] AC-006: Given a trade with 5 closed sibling trades in the same portfolio, when opening trade detail, then PortfolioImpact shows sum of PnL
- [ ] AC-007: Given a trade with 3 closed sibling trades in the same strategy, when opening trade detail, then StrategyImpact shows sum of PnL
- [ ] AC-008: Given PortfolioDetail loaded with "3M" period selected, when page loads, then equity curve shows 90 days of data
- [ ] AC-009: Given PortfolioDetail, when changing period to "1W", then all charts and data reload with 7-day range

## Test Mapping

| Acceptance Criteria | Test File | Test Method |
|---------------------|-----------|-------------|
| AC-005 | TradeServiceTests.cs | GetTradesAsync_Should_Resolve_StrategyName |
| AC-006-007 | TradeServiceTests.cs | GetTradeDetailAsync_Should_Compute_Impacts |
| AC-008-009 | (manual UI test) | |

## UI Components

- `Home.razor` — overview dashboard with KPIs, connections, quick actions
- `PlatformDashboard.razor` — platform connection management page
- `NavMenu.razor` — updated with Platform link
- `PortfolioDetail.razor` — updated with period selector and proper reload

## Dependencies

- Domain: `PlatformConnectionStatus`, `StrategyDefinition`
- Application: `IPlatformService`, `PlatformDashboardDto`, `PlatformConnectionDto`
- Application: `IStrategyDefinitionRepository` (new dependency in TradeService)
- Infrastructure: `IPlatformDataRepository` (stub implementation added)
