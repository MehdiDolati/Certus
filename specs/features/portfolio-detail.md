# Portfolio Detail Report

## Status
Active

## Overview
Detailed report for a single portfolio with equity curve, drawdown chart, monthly heatmap, strategy performance, and trade log.

## Route
`/portfolios/{id}`

## Requirements

### Functional Requirements
- [ ] FR-001: Display portfolio header with name, status, created date, strategy count
- [ ] FR-002: Display 9 KPI cards scoped to portfolio: Actual Return, Supposed Return, Variance, Sharpe, Sortino, Max Drawdown, Calmar, Win Rate, Profit Factor
- [ ] FR-003: Display equity curve chart (actual vs supposed lines)
- [ ] FR-004: Display drawdown chart (underwater equity)
- [ ] FR-005: Display monthly returns heatmap with color scale
- [ ] FR-006: Display strategy performance table with contribution %
- [ ] FR-007: Display strategy PnL stacked area chart
- [ ] FR-008: Display trade log with pagination (20/50/100 per page)
- [ ] FR-009: Support filtering trades by strategy, symbol, side, status
- [ ] FR-010: Clicking a trade opens detail modal
- [ ] FR-011: Display cumulative PnL chart
- [ ] FR-012: Back button navigates to dashboard

### Non-Functional Requirements
- [ ] NFR-001: Detail report load < 1.5 seconds
- [ ] NFR-002: Trade log pagination < 500ms per page
- [ ] NFR-003: Chart render < 1 second

## Data Model

### Input
- Portfolio ID (Guid from route)
- `DateRange` period filter

### Output
- `PortfolioDetailDto` — summary + strategies + equity curve + drawdown curve + monthly returns
- `PaginatedResult<TradeDto>` — paginated trade log
- `List<PerformanceSnapshotDto>` — cumulative PnL data

## Acceptance Criteria

- [ ] AC-001: Given a valid portfolio ID, when loading detail, then portfolio header shows correct name and status
- [ ] AC-002: Given a portfolio with no strategies, when loading detail, then "No strategies" message is shown
- [ ] AC-003: Given a portfolio with 90 days of data, when viewing equity curve, then 90 data points are plotted
- [ ] AC-004: Given trade log with 50 trades, when paginating with 20 per page, then page 1 shows 20 trades
- [ ] AC-005: Given an invalid GUID, when loading detail, then redirect to dashboard with error toast

## Test Mapping

| Acceptance Criteria | Test File | Test Method |
|---------------------|-----------|-------------|
| AC-001 | PortfolioServiceTests.cs | GetPortfolioDetailAsync_Should_Return_Null_When_Portfolio_Not_Found |
| AC-002-005 | (manual UI test) | |

## UI Components

- `PortfolioDetail.razor` — main page
- `EquityCurveChart.razor` — line chart
- `DrawdownChart.razor` — area chart
- `MonthlyReturnsHeatmap.razor` — color grid
- `StrategyPerformanceTable.razor` — data table
- `StrategyPnlStackedChart.razor` — stacked area chart
- `TradeLogTable.razor` — paginated table
- `TradeDetailModal.razor` — detail dialog
- `CumulativePnlChart.razor` — line chart

## Dependencies

- Domain: `Portfolio`, `Strategy`, `Trade`, `PerformanceSnapshot`
- Application: `IPortfolioService`, `ITradeService`, DTOs
