# Portfolio Dashboard

## Status
Active

## Overview
The Portfolio Dashboard is the main landing page showing all portfolios with KPI cards, a performance comparison table, and filtering/sorting capabilities.

## Route
`/portfolios`

## Requirements

### Functional Requirements
- [ ] FR-001: Display 6 KPI cards: Total AUM, Total PnL, Avg Sharpe, Max Drawdown, Win Rate, Active Count
- [ ] FR-002: Display portfolio comparison table with columns: Name, Actual Return, Supposed Return, Variance, Sharpe, Max Drawdown, Trade Count
- [ ] FR-003: Support date range filtering (1W, 1M, 3M, 6M, YTD, 1Y, All)
- [ ] FR-004: Support status filtering (All, Active, Paused, Closed)
- [ ] FR-005: Support search by portfolio name
- [ ] FR-006: Support sorting by any column (ascending/descending)
- [ ] FR-007: Clicking a portfolio navigates to `/portfolios/{id}`

### Non-Functional Requirements
- [ ] NFR-001: Initial load < 2 seconds with 50 portfolios
- [ ] NFR-002: Debounced search with 300ms delay

## Data Model

### Input
- `PortfolioFilter` record with Status, MinAum, Search, SortBy, Ascending, Period

### Output
- `List<PortfolioDto>` — portfolio summaries
- `DashboardKpis` — aggregated KPIs

## Acceptance Criteria

- [ ] AC-001: Given no portfolios exist, when loading dashboard, then empty state message is displayed
- [ ] AC-002: Given 5 active portfolios, when loading dashboard, then Active Count shows 5
- [ ] AC-003: Given portfolios with mixed statuses, when filtering by Active, then only active portfolios are shown
- [ ] AC-004: Given a portfolio with 24.3% actual return and 18.0% supposed return, when viewing table, then variance shows +6.3%
- [ ] AC-005: Given search query "Alpha", when searching, then only portfolios with "Alpha" in name are shown
- [ ] AC-006: Given default sort by Variance descending, when loading dashboard, then highest variance portfolio is first

## Test Mapping

| Acceptance Criteria | Test File | Test Method |
|---------------------|-----------|-------------|
| AC-001 | PortfolioServiceTests.cs | GetAllPortfoliosAsync_Should_Return_Empty_When_No_Portfolios |
| AC-002 | PortfolioServiceTests.cs | GetDashboardKpisAsync_Should_Return_Zero_KPIs_When_No_Data |
| AC-003-006 | (manual UI test) | |

## UI Components

- `PortfolioDashboard.razor` — main page
- `KpiCard.razor` — KPI metric card
- `PortfolioSummaryTable.razor` — data table
- `PerformanceComparisonBar.razor` — visual comparison bar

## Dependencies

- Domain: `Portfolio`, `PortfolioStatus`, `DateRange`
- Application: `IPortfolioService`, `PortfolioDto`, `DashboardKpis`, `PortfolioFilter`
