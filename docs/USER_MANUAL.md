# Certus User Manual

> **Version:** 0.1.0 (Alpha)
> **Last Updated:** 2026-07-08
> **Status:** Domain Architecture Restructured — 6 Bounded Contexts, TDD, Spec-Driven Development

---

## Table of Contents

- [1. Welcome to Certus](#1-welcome-to-certus)
- [2. What Certus Does](#2-what-certus-does)
- [3. Getting Started](#3-getting-started)
  - [3.1 Prerequisites](#31-prerequisites)
  - [3.2 Installation Options](#32-installation-options)
  - [3.3 First Launch](#33-first-launch)
- [4. The Dashboard](#4-the-dashboard)
  - [4.1 Home (Overview)](#41-home-overview)
  - [4.2 Portfolio Dashboard](#42-portfolio-dashboard)
  - [4.3 Portfolio Detail Report](#43-portfolio-detail-report)
  - [4.4 Platform Dashboard](#44-platform-dashboard)
- [5. Managing Portfolios](#5-managing-portfolios)
  - [5.1 Creating a Portfolio](#51-creating-a-portfolio)
  - [5.2 Editing a Portfolio](#52-editing-a-portfolio)
  - [5.3 Deleting a Portfolio](#53-deleting-a-portfolio)
  - [5.4 Portfolio Status](#54-portfolio-status)
- [6. Managing Strategies](#6-managing-strategies)
  - [6.1 Adding a Strategy](#61-adding-a-strategy)
  - [6.2 Editing a Strategy](#62-editing-a-strategy)
  - [6.3 Strategy Status](#63-strategy-status)
  - [6.4 Strategy Weight](#64-strategy-weight)
- [7. Understanding Trades](#7-understanding-trades)
  - [7.1 Trade Lifecycle](#71-trade-lifecycle)
  - [7.2 Reading the Trade Log](#72-reading-the-trade-log)
  - [7.3 PnL Calculation](#73-pnl-calculation)
- [8. Platform Integration](#8-platform-integration)
  - [8.1 Overview](#81-overview)
  - [8.2 MetaTrader 4 Setup](#82-metatrader-4-setup)
  - [8.3 Connecting Certus to MT4](#83-connecting-certus-to-mt4)
  - [8.4 Data Import](#84-data-import)
  - [8.5 Adding New Platforms](#85-adding-new-platforms)
- [9. Dashboard Charts & KPIs](#9-dashboard-charts--kpis)
  - [9.1 KPI Cards](#91-kpi-cards)
  - [9.2 Equity Curve](#92-equity-curve)
  - [9.3 Drawdown Chart](#93-drawdown-chart)
  - [9.4 Monthly Returns Heatmap](#94-monthly-returns-heatmap)
  - [9.5 Strategy PnL Stacked Chart](#95-strategy-pnl-stacked-chart)
  - [9.6 Cumulative PnL Chart](#96-cumulative-pnl-chart)
- [10. Configuration](#10-configuration)
  - [10.1 Application Settings](#101-application-settings)
  - [10.2 Docker Configuration](#102-docker-configuration)
- [11. Troubleshooting](#11-troubleshooting)
- [12. Glossary](#12-glossary)
- [13. Further Reading](#13-further-reading)

---

## 1. Welcome to Certus

Certus is an autonomous multi-agent trading platform designed to deliver predictable profitability through AI agents. It evolves incrementally from human-supervised decision support to fully autonomous trading, integrating market intelligence, strategy generation, simulation, risk governance, trade execution, and continuous performance optimization.

**Key Principles:**

- **Spec-driven development** — every feature starts with a written specification
- **Clean Architecture** — strict layer boundaries enforced by automated tests
- **Test-first** — 100% coverage targets for business logic layers
- **Platform-agnostic** — connects to MetaTrader 4 today, extensible to other platforms tomorrow

---

## 2. What Certus Does

Certus manages the full trading lifecycle:

```
Market Data → Strategy Design → Backtesting → Risk Assessment → Trade Execution → Evaluation
     ↑                                                                                   |
     └──────────────────────── Continuous Feedback Loop ──────────────────────────────────┘
```

| Capability | Description |
|------------|-------------|
| **Portfolio Management** | Create, monitor, and manage multiple trading portfolios with different strategies |
| **Strategy Tracking** | Define strategies with parameters, weights, and performance targets |
| **Trade Monitoring** | View all trades with full lifecycle data (entry, exit, PnL, fees) |
| **Performance Analytics** | Equity curves, drawdown charts, monthly heatmaps, risk metrics |
| **Platform Integration** | Import live data from MetaTrader 4 via Expert Advisor |
| **Risk Management** | Configurable risk limits, capital allocation, drawdown alerts |
| **Backtesting** | Simulate strategies against historical data |
| **Agent Coordination** | AI agent task management and health monitoring |

---

## 3. Getting Started

### 3.1 Prerequisites

| Requirement | Version | Notes |
|-------------|---------|-------|
| .NET SDK | 10.0+ | For building and running from source |
| Docker | Latest | For containerized deployment |
| MetaTrader 4 | Latest | Only needed for live platform integration |

### 3.2 Installation Options

#### Option A: Run from Source (Development)

```bash
# Clone the repository
git clone https://github.com/your-org/certus.git
cd Certus

# Restore and build
dotnet restore
dotnet build Certus.slnx

# Run the application
dotnet run --project src/Certus.Dashboard
```

The dashboard will be available at `http://localhost:5000` (or the port shown in the console output).

#### Option B: Docker (Production)

```bash
# Start both SQL Server and the Dashboard
docker compose up --build
```

The dashboard will be available at `http://localhost:8080`.

**What this starts:**

| Service | Port | Description |
|---------|------|-------------|
| `dashboard` | 8080 | Certus Blazor Server application |
| `sqlserver` | 1433 | Microsoft SQL Server 2022 (Developer) |

### 3.3 First Launch

1. Open your browser to `http://localhost:8080` (Docker) or `http://localhost:5000` (source)
2. You'll see the **Home** page with overview metrics
3. Seed data is automatically loaded: 5 sample portfolios, 12 strategies, and ~158 trades
4. Navigate to **Portfolios** to explore the dashboard

---

## 4. The Dashboard

The dashboard is built with Blazor Server and MudBlazor, featuring a dark theme optimized for trading data visualization.

### 4.1 Home (Overview)

**Route:** `/`

The Home page shows a high-level overview of all trading activity:

- Total portfolios and their statuses
- Aggregate KPIs across all portfolios
- Quick links to portfolio details

### 4.2 Portfolio Dashboard

**Route:** `/portfolios`

The main landing page for portfolio management.

**Features:**

| Feature | Description |
|---------|-------------|
| **KPI Cards** | Total AUM, Total PnL, Avg Sharpe, Max Drawdown, Win Rate, Active Count |
| **Performance Table** | Name, Actual Return, Supposed Return, Variance, Sharpe, Max Drawdown, Trade Count |
| **Date Filtering** | 1W, 1M, 3M, 6M, YTD, 1Y, All |
| **Status Filtering** | All, Active, Paused, Closed |
| **Search** | Debounced search by portfolio name (300ms delay) |
| **Sorting** | Click any column header to sort ascending/descending |
| **Navigation** | Click a portfolio name to open its detail report |

### 4.3 Portfolio Detail Report

**Route:** `/portfolios/{id}`

A comprehensive report for a single portfolio.

**Sections:**

1. **Portfolio Header** — Name, status, creation date, strategy count
2. **KPI Cards (9)** — Actual Return, Supposed Return, Variance, Sharpe, Sortino, Max Drawdown, Calmar, Win Rate, Profit Factor
3. **Equity Curve Chart** — Actual vs Supposed return lines over time
4. **Drawdown Chart** — Underwater equity visualization
5. **Monthly Returns Heatmap** — Color-coded grid showing monthly performance
6. **Strategy Performance Table** — Per-strategy contribution and metrics
7. **Strategy PnL Stacked Chart** — Stacked area chart of strategy contributions
8. **Trade Log** — Paginated table with filtering by strategy, symbol, side, status
9. **Trade Detail Modal** — Click any trade for full details
10. **Cumulative PnL Chart** — Running profit/loss line chart

### 4.4 Platform Dashboard

**Route:** `/platform`

Shows connected trading platforms and their import status.

**Features:**

- Platform connection status (Connected, Disconnected, Error)
- Last data received timestamp
- Import controls for portfolios and trades
- Deviation analysis between expected and actual trade outcomes

---

## 5. Managing Portfolios

### 5.1 Creating a Portfolio

1. Navigate to **Portfolios**
2. Click the **Create** button (top-right)
3. Fill in the form:

| Field | Required | Description |
|-------|----------|-------------|
| Name | Yes | Display name (max 200 characters) |
| Description | No | Optional description (max 1000 characters) |
| Target Return | No | Annual target return as percentage (e.g., 25 = 25%) |
| Target Sharpe | No | Target Sharpe ratio |
| Allocated Capital | No | USD amount allocated to this portfolio |

4. Click **Save**

**Note:** Manually created portfolios start with a non-Active status. Portfolios imported from a connected platform can be Active if the platform reports them as live.

### 5.2 Editing a Portfolio

1. Navigate to **Portfolios**
2. Click the **Edit** button (pencil icon) on any portfolio row
3. Modify fields as needed
4. Click **Save**

### 5.3 Deleting a Portfolio

Deleting a portfolio is a **soft delete** — the portfolio status changes to Closed and it is hidden from the active view.

1. Navigate to **Portfolios**
2. Click the **Delete** button (trash icon) on any portfolio row
3. Confirm the deletion

**Important:** Closed portfolios cannot be reopened.

### 5.4 Portfolio Status

| Status | Description |
|--------|-------------|
| **Active** | Portfolio is operational, can accept new strategies |
| **Paused** | Temporarily suspended — strategies exist but no new trades |
| **Closed** | Permanently closed, no modifications allowed |

**Status transitions:**
- Active → Paused
- Paused → Active
- Active → Closed
- Paused → Closed

---

## 6. Managing Strategies

### 6.1 Adding a Strategy

1. Open a **Portfolio Detail** report (click any portfolio)
2. Click **New Strategy**
3. Fill in the form:

| Field | Required | Description |
|-------|----------|-------------|
| Name | Yes | Display name (max 200 characters) |
| Type | No | Strategy type (e.g., Momentum, MeanReversion, Arbitrage) |
| Target Return | No | Target annual return percentage |
| Weight | Yes | Allocation weight within portfolio (0.0 to 1.0) |

4. Click **Save**

**Note:** The "New Strategy" button is disabled for portfolios that are not Active.

### 6.2 Editing a Strategy

1. Open a **Portfolio Detail** report
2. Find the strategy in the strategy table
3. Click the **Edit** button
4. Modify fields and click **Save**

### 6.3 Strategy Status

| Status | Description |
|--------|-------------|
| **Active** | Strategy is actively trading |
| **Paused** | Temporarily suspended |
| **Retired** | Permanently decommissioned |

### 6.4 Strategy Weight

Each strategy has a **weight** (0.0 to 1.0) that determines its allocation within the portfolio.

- Weights should sum to 1.0 across all strategies in a portfolio
- Example: If portfolio has 3 strategies with weights 0.5, 0.3, and 0.2, they get 50%, 30%, and 20% of capital respectively

---

## 7. Understanding Trades

### 7.1 Trade Lifecycle

```
Open → (active position) → Closed
                ↘ Cancelled
```

| Status | Description |
|--------|-------------|
| **Open** | Position is active, currently in the market |
| **Closed** | Position has been closed, PnL realized |
| **Cancelled** | Trade was cancelled before execution |

### 7.2 Reading the Trade Log

The trade log shows all trades for a portfolio with these columns:

| Column | Description |
|--------|-------------|
| Symbol | Trading instrument (e.g., EURUSD, BTCUSD) |
| Side | Long (buy) or Short (sell) |
| Entry Price | Price when the trade was opened |
| Exit Price | Price when the trade was closed (blank if open) |
| Quantity | Position size |
| PnL | Profit or loss in USD |
| PnL % | Percentage return |
| Fees | Total fees paid |
| Status | Open, Closed, or Cancelled |

**Filtering:** Filter by strategy, symbol, side, or status.
**Pagination:** 20, 50, or 100 trades per page.

### 7.3 PnL Calculation

**Long trades (buy):**
```
PnL = (ExitPrice - EntryPrice) × Quantity
```

**Short trades (sell):**
```
PnL = (EntryPrice - ExitPrice) × Quantity
```

**PnL Percentage:**
```
PnL% = PnL / (EntryPrice × Quantity) × 100
```

---

## 8. Platform Integration

### 8.1 Overview

Certus is platform-agnostic. It reads data from external trading platforms via a **plugin architecture**:

```
Trading Platform (MT4/MT5/Broker) → Plugin Adapter → Certus
                                                        ↓
                                              Portfolio / Strategy / Trade data
```

**Key design principle:** Certus **never writes to or modifies** platform files. All file operations are read-only.

### 8.2 MetaTrader 4 Setup

The MetaTrader 4 integration uses an Expert Advisor (EA) that writes trade data to shared JSON files.

#### EA Architecture

```
MetaTrader4/Experts/CertusEA.mq4
├── CertusEA.mq4                    # Main EA entry point
├── Include/
│   ├── CertusConfig.mqh           # Input parameters
│   ├── CertusFileWriter.mqh       # Atomic file write utilities
│   ├── CertusPortfolioStatus.mqh  # Portfolio status builder
│   ├── CertusTradeLogger.mqh      # Trade event logging
│   └── CertusStrategyInfo.mqh     # Strategy data collection
└── Files/Certus/                   # Output directory
    ├── portfolio_status.json       # Portfolio + strategy status
    └── trades.json                 # Trade event log
```

#### EA Input Parameters

| Parameter | Default | Description |
|-----------|---------|-------------|
| OutputDirectory | "Certus" | Subdirectory under MT4 Files/ |
| UpdateIntervalSeconds | 5 | How often to write portfolio_status.json |
| LogAllTrades | true | Log trades on open/close/modify |
| LogLevel | "INFO" | INFO, DEBUG, WARN, ERROR |

#### Key Features

- **Single instance monitoring:** One EA on any chart monitors ALL account activity
- **Magic number grouping:** Trades are grouped by magic number to identify strategies
- **Manual trade tracking:** Magic number 0 represents manual trading
- **Atomic file writes:** Uses temp files and rename to prevent partial reads

### 8.3 Connecting Certus to MT4

1. **Install the EA:**
   - Copy `CertusEA.mq4` to your MT4 `Experts/` folder
   - Compile in MetaEditor
   - Attach to any chart

2. **Configure the EA:**
   - Set `OutputDirectory` to the shared folder path
   - Set `UpdateIntervalSeconds` (default: 5 seconds)

3. **Configure Certus:**
   - Navigate to the **Platform** dashboard
   - Click **Connect** and select MetaTrader 4
   - Enter the shared folder path
   - Click **Connect**

4. **Verify:**
   - Connection status should show "Connected"
   - Portfolio data should appear in the dashboard

### 8.4 Data Import

**Portfolio Import:**
1. Connect to the platform
2. Select a portfolio from the platform's account list
3. Click **Import** — a Certus portfolio is created with `IsPlatformManaged = true`

**Trade Import:**
1. Select a strategy within an imported portfolio
2. Click **Import Trades**
3. Choose date range (optional)
4. Trades are imported with deduplication (same ExternalId is skipped)

**Real-time Monitoring:**
- Certus uses `FileSystemWatcher` to monitor for file changes
- Change detection latency: < 200ms
- Portfolio status is overwritten each update cycle (latest state)
- Trade log is append-only (never overwritten)

### 8.5 Adding New Platforms

Certus supports adding new trading platforms via the plugin architecture.

**Steps:**

1. Create a new class library project
2. Implement the `IPlatformPlugin` interface
3. Implement the adapter that reads platform-specific data
4. Place the compiled DLL in the plugins directory
5. Restart Certus — the plugin is loaded automatically

**Plugin interface requirements:**

```csharp
public interface IPlatformPlugin
{
    string PlatformId { get; }
    string PlatformName { get; }
    IPlatformAdapter CreateAdapter(PlatformConfig config);
}
```

Failed plugins are logged but don't crash the application.

---

## 9. Dashboard Charts & KPIs

### 9.1 KPI Cards

**Portfolio Dashboard KPIs:**

| KPI | Description |
|-----|-------------|
| Total AUM | Total Assets Under Management across all portfolios |
| Total PnL | Combined profit/loss |
| Avg Sharpe | Average Sharpe ratio (risk-adjusted return) |
| Max Drawdown | Worst peak-to-trough decline |
| Win Rate | Percentage of profitable trades |
| Active Count | Number of currently active portfolios |

**Portfolio Detail KPIs:**

| KPI | Description |
|-----|-------------|
| Actual Return | Actual return percentage |
| Supposed Return | Target/benchmark return percentage |
| Variance | Difference between actual and supposed return |
| Sharpe | Sharpe ratio |
| Sortino | Sortino ratio (downside risk-adjusted) |
| Max Drawdown | Maximum drawdown percentage |
| Calmar | Calmar ratio (return/max drawdown) |
| Win Rate | Percentage of winning trades |
| Profit Factor | Gross profit / gross loss |

### 9.2 Equity Curve

A line chart showing:
- **Actual return** (solid line) — your portfolio's real performance
- **Supposed return** (dashed line) — the target or benchmark

The gap between lines shows whether you're outperforming or underperforming.

### 9.3 Drawdown Chart

An area chart showing the portfolio's underwater equity — how far the portfolio has fallen from its peak.

- Values are negative (representing losses from peak)
- Useful for understanding risk and recovery time

### 9.4 Monthly Returns Heatmap

A color-coded grid where:
- **Green** = positive month
- **Red** = negative month
- **Intensity** = magnitude of return

Rows are years, columns are months (Jan–Dec).

### 9.5 Strategy PnL Stacked Chart

A stacked area chart showing each strategy's contribution to total portfolio PnL over time.

- Each layer represents one strategy
- Layer height = strategy's PnL contribution
- Total height = portfolio's total PnL

### 9.6 Cumulative PnL Chart

A line chart showing the running total of profit/loss over time.

---

## 10. Configuration

### 10.1 Application Settings

Key settings in `appsettings.json`:

| Setting | Default | Description |
|---------|---------|-------------|
| `ConnectionStrings:DefaultConnection` | SQLite / SQL Server | Database connection string |
| `ASPNETCORE_ENVIRONMENT` | Development | Environment (Development, Production) |

### 10.2 Docker Configuration

The `docker-compose.yml` defines:

| Service | Image | Port | Purpose |
|---------|-------|------|---------|
| `sqlserver` | `mcr.microsoft.com/mssql/server:2022-latest` | 1433 | SQL Server database |
| `dashboard` | Built from `Dockerfile` | 8080 | Certus application |

**Persistent storage:** SQL Server data is stored in a Docker volume (`sqlserver-data`).

**Environment variables:**

```yaml
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Server=sqlserver;Database=Certus;...
```

---

## 11. Troubleshooting

### Common Issues

| Issue | Solution |
|-------|----------|
| **Port already in use** | Change the port in `docker-compose.yml` or `launchSettings.json` |
| **SQL Server won't start** | Check if port 1433 is already in use: `netstat -ano | findstr 1433` |
| **Build fails** | Run `dotnet restore` first, then `dotnet build` |
| **Dashboard shows no data** | Ensure seed data is loaded; check `SeedData.cs` configuration |
| **MT4 connection fails** | Verify the shared folder path is accessible and the EA is running |
| **File locking errors** | The EA uses atomic writes (temp file + rename); ensure write permissions |
| **Plugin not loading** | Check that the DLL implements `IPlatformPlugin`; check logs for errors |

### Logs

- Application logs are written to the console (development) or stdout (Docker)
- MT4 EA logs are written to MT4's Experts log tab
- Check Docker logs: `docker compose logs dashboard`

### Running Tests

```bash
# All tests
dotnet test Certus.slnx

# Specific test projects
dotnet test tests/Certus.Domain.Tests/
dotnet test tests/Certus.Application.Tests/
dotnet test tests/Certus.IntegrationTests/
dotnet test tests/Certus.ArchitectureTests/
```

---

## 12. Glossary

| Term | Definition |
|------|------------|
| **AUM** | Assets Under Management — total value of managed capital |
| **PnL** | Profit and Loss — the gain or loss from trading activity |
| **Sharpe Ratio** | Risk-adjusted return measure (higher is better) |
| **Sortino Ratio** | Like Sharpe but only penalizes downside volatility |
| **Max Drawdown** | Largest peak-to-trough decline before a new peak |
| **Win Rate** | Percentage of trades that are profitable |
| **Profit Factor** | Ratio of gross profits to gross losses |
| **Calmar Ratio** | Annualized return divided by max drawdown |
| **Weight** | Allocation percentage of a strategy within a portfolio |
| **Magic Number** | MT4 identifier that groups trades by strategy |
| **Aggregate Root** | Domain-Driven Design term for the entry point to an aggregate |
| **Value Object** | An immutable object defined by its attributes (not identity) |
| **Domain Event** | A message raised by an entity to signal something happened |
| **Bounded Context** | A logical boundary within which a particular domain model applies |
| **Clean Architecture** | Architecture pattern with strict dependency rules (inner layers don't know outer layers) |

---

## 13. Further Reading

| Document | Location | Description |
|----------|----------|-------------|
| Project README | `README.md` | Project overview |
| Specification Index | `SPEC.md` | Links to all feature specs |
| Architecture Plan | `ARCHITECTURE_PLAN.md` | Layer rules, dependency constraints |
| Development Plan | `DEVELOPMENT_PLAN.md` | Phases, branching, commit conventions |
| Spec-Driven Workflow | `specs/README.md` | How specs map to tests |
| Domain Specs | `specs/domain/` | Entity rules, invariants |
| Feature Specs | `specs/features/` | Feature specifications |
| Architecture Specs | `specs/architecture/` | Architecture rules |

---

*This manual is a living document. As new features are implemented, this manual will be updated accordingly.*
