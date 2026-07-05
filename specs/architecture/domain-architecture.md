# Certus Domain Architecture — Bounded Contexts for Autonomous Agents

## Status
Proposed

## Overview

This document defines the bounded context architecture for the Certus automated trading platform. The design maps 6 functional units (future autonomous agents) to 6 bounded contexts, structured for a phased evolution: modular monolith (Phase 1) → full DDD with separate DbContexts (Phase 2) → autonomous agent services (Phase 3).

---

## 1. Bounded Context Map

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        Certus Platform                                  │
│                                                                         │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐              │
│  │   Market      │    │   Strategy   │    │  Execution   │              │
│  │  Intelligence │───▶│   Design &   │───▶│    Trade     │              │
│  │   Context     │    │  Simulation  │    │  Execution   │              │
│  └──────────────┘    │   Context    │    │   Context    │              │
│         │            └──────────────┘    └──────────────┘              │
│         │                   │                    │                      │
│         ▼                   ▼                    ▼                      │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐              │
│  │    Risk &     │    │  Evaluation  │    │   Coordina-  │              │
│  │   Portfolio   │◀──│  & Feedback  │    │    tion      │              │
│  │   Context     │    │   Context    │    │   Context    │              │
│  └──────────────┘    └──────────────┘    └──────────────┘              │
│                                                                         │
│  ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─  │
│                     SharedKernel (Cross-cutting)                        │
└─────────────────────────────────────────────────────────────────────────┘
```

### Data Flow (Happy Path)

1. **Market Intelligence** gathers data → produces `SignalPublished` events
2. **Strategy Design** consumes signals → evaluates strategy rules → emits `TradeSignalGenerated`
3. **Risk & Portfolio** validates risk limits → emits `OrderApproved`
4. **Trade Execution** places orders → emits `TradeExecuted` / `TradeClosed`
5. **Evaluation & Feedback** computes performance → emits `PerformanceEvaluated`
6. **Coordination** orchestrates the cycle, monitors agent health, adjusts parameters

---

## 2. Bounded Context Designs

### 2.1 Market Intelligence Context

**Responsibility**: Market data ingestion, technical/fundamental analysis, signal generation.

**Agent mapping**: Market Intelligence Agent

#### Aggregate Roots

| Aggregate Root | Description |
|----------------|-------------|
| `MarketDataSeries` | Time-series data for a symbol (OHLCV bars, ticks). Root of the data aggregate. |

#### Entities

| Entity | Parent | Description |
|--------|--------|-------------|
| `PriceBar` | MarketDataSeries | Individual OHLCV bar with timestamp |
| `TechnicalIndicator` | MarketDataSeries | Computed indicator values (RSI, MACD, BB, etc.) |

#### Value Objects

| Value Object | Fields | Description |
|--------------|--------|-------------|
| `Symbol` | `Value`, `AssetClass` | Trading pair identifier (e.g., "BTC/USD", AssetClass.Crypto) |
| `PriceRange` | `Low`, `High` | Price range within a period |
| `VolumeProfile` | `TotalVolume`, `BuyVolume`, `SellVolume` | Volume breakdown |
| `SignalStrength` | `Value` (0.0–1.0), `Confidence` | How strong/convincing a signal is |
| `TimeFrame` | `Value` (e.g., "1h", "4h", "1d") | Analysis timeframe |

#### Enums

```csharp
public enum AssetClass { Forex, Crypto, CFD, Futures }
public enum SignalDirection { Bullish, Bearish, Neutral }
public enum IndicatorType { RSI, MACD, BollingerBands, ATR, OBV, Custom }
public enum FundamentalFactorType { FundingRate, OpenInterest, SocialSentiment, OnChainMetric }
```

#### Domain Events

| Event | Trigger | Consumers |
|-------|---------|-----------|
| `MarketDataReceived` | New price data ingested | Strategy, Evaluation |
| `SignalPublished` | Technical/fundamental signal generated | Strategy, Coordination |
| `AnomalyDetected` | Unusual market condition detected | Risk, Coordination |

#### Repository Interfaces

```csharp
public interface IMarketDataRepository
{
    Task<MarketDataSeries?> GetBySymbolAsync(Symbol symbol, TimeFrame timeframe);
    Task<IReadOnlyList<PriceBar>> GetBarsAsync(Symbol symbol, TimeFrame timeframe, DateTime from, DateTime to);
    Task AddOrUpdateAsync(MarketDataSeries series);
}

public interface ISignalRepository
{
    Task<IReadOnlyList<Signal>> GetActiveSignalsAsync(Symbol? symbol, SignalDirection? direction);
    Task AddAsync(Signal signal);
}
```

---

### 2.2 Strategy Design & Simulation Context

**Responsibility**: Strategy definition, backtesting, simulation, strategy lifecycle management.

**Agent mapping**: Strategy Design & Simulation Agent

#### Aggregate Roots

| Aggregate Root | Description |
|----------------|-------------|
| `StrategyDefinition` | The strategy itself — rules, parameters, configuration. Aggregate boundary ensures weight/slot consistency within a portfolio. |

#### Entities

| Entity | Parent | Description |
|--------|--------|-------------|
| `StrategyParameter` | StrategyDefinition | Named parameter with type, default, range (e.g., "LookbackPeriod", int, 20, 5–100) |
| `BacktestRun` | StrategyDefinition | A single backtest execution with results |
| `BacktestTrade` | BacktestRun | Individual trade within a backtest |
| `StrategySlot` | StrategyDefinition | Assignment to a portfolio with weight allocation |

#### Value Objects

| Value Object | Fields | Description |
|--------------|--------|-------------|
| `StrategyType` | `Category` (Momentum, MeanReversion, Arbitrage, TrendFollowing, StatisticalArb, Custom), `SubType` | Typed strategy classification (replaces free-form string) |
| `BacktestConfig` | `StartDate`, `EndDate`, `InitialCapital`, `Symbols[]`, `TimeFrame` | Backtest parameters |
| `BacktestMetrics` | `TotalReturn`, `SharpeRatio`, `MaxDrawdown`, `WinRate`, `ProfitFactor`, `CalmarRatio`, `SortinoRatio`, `TotalTrades`, `AvgTradeDuration` | Performance metrics from a backtest |
| `Weight` | `Value` (0.0–1.0) | Allocation weight within a portfolio (validated) |
| `RiskProfile` | `MaxDrawdownLimit`, `MaxPositionSize`, `VolatilityTarget` | Strategy-level risk constraints |
| `ConfidenceInterval` | `Lower`, `Upper`, `ConfidenceLevel` | Statistical confidence range for predicted returns |

#### Enums

```csharp
public enum StrategyStatus { Draft, Backtesting, Active, Paused, Retired }
public enum BacktestStatus { Pending, Running, Completed, Failed }
```

#### Domain Events

| Event | Trigger | Consumers |
|-------|---------|-----------|
| `StrategyCreated` | New strategy definition saved | Coordination |
| `BacktestCompleted` | Backtest run finished with results | Evaluation, Coordination |
| `StrategyActivated` | Strategy moved from Draft/Backtesting to Active | Risk, Execution, Coordination |
| `StrategyDeactivated` | Strategy paused or retired | Execution, Risk, Coordination |
| `StrategyParametersUpdated` | Strategy parameters changed | Execution, Coordination |
| `WeightAdjusted` | Strategy weight in a portfolio changed | Risk, Coordination |

#### Repository Interfaces

```csharp
public interface IStrategyDefinitionRepository
{
    Task<StrategyDefinition?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<StrategyDefinition>> GetByPortfolioIdAsync(Guid portfolioId);
    Task<IReadOnlyList<StrategyDefinition>> GetByStatusAsync(StrategyStatus status);
    Task AddAsync(StrategyDefinition strategy);
    void Update(StrategyDefinition strategy);
}

public interface IBacktestRepository
{
    Task<BacktestRun?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<BacktestRun>> GetByStrategyIdAsync(Guid strategyId);
    Task AddAsync(BacktestRun run);
}
```

#### Relationship to Existing `Strategy` Entity

The existing `Strategy` entity (with `PortfolioId`, `Weight`, `Type` as string) is **refactored** into `StrategyDefinition`:

- `Strategy.Type` (string) → `StrategyDefinition.StrategyType` (value object with `Category` + `SubType`)
- `Strategy.Weight` (decimal) → Stored in `StrategySlot` (the portfolio assignment)
- `Strategy.PortfolioId` → Via `StrategySlot.PortfolioId` (a strategy can be assigned to multiple portfolios)
- New: `StrategyParameter[]` for configurable parameters
- New: `BacktestRun[]` for simulation history
- New: `RiskProfile` for strategy-level risk limits

---

### 2.3 Risk & Portfolio Context

**Responsibility**: Portfolio management, capital allocation, risk assessment, adaptive risk limits.

**Agent mapping**: Risk & Portfolio Management Agent

#### Aggregate Roots

| Aggregate Root | Description |
|----------------|-------------|
| `Portfolio` | Top-level portfolio aggregate. Encapsulates capital allocation and risk limits. Ensures total allocation consistency. |

#### Entities

| Entity | Parent | Description |
|--------|--------|-------------|
| `RiskLimit` | Portfolio | Individual risk constraint (max drawdown, max position, VaR limit) |
| `AllocationRule` | Portfolio | Rules for how capital is distributed across strategies |
| `RiskAssessment` | Portfolio | Point-in-time risk evaluation snapshot |
| `CapitalAllocation` | Portfolio | Current capital distribution across strategies |

#### Value Objects

| Value Object | Fields | Description |
|--------------|--------|-------------|
| `Money` | `Amount`, `Currency` | Monetary value with currency (replaces bare decimals for capital) |
| `RiskLimitValue` | `Type`, `Threshold`, `Current`, `IsBreached` | A single risk metric with current state |
| `DrawdownLimit` | `MaxDrawdown`, `WarningLevel`, `HaltLevel` | Tiered drawdown thresholds |
| `PositionLimit` | `MaxPositionSize`, `MaxLeverage`, `MaxConcurrentPositions` | Position constraints |
| `VaRMetric` | `Value`, `ConfidenceLevel`, `TimeHorizon` | Value at Risk measurement |
| `AllocationTarget` | `StrategyId`, `TargetWeight`, `MinWeight`, `MaxWeight` | Target allocation for a strategy |
| `AdaptiveRiskParams` | `VolatilityScalingFactor`, `ConfidenceThreshold`, `MarketRegime` | Dynamic risk parameters that adjust based on conditions |

#### Enums

```csharp
public enum PortfolioStatus { Active, Paused, Closed }
public enum RiskLimitType { MaxDrawdown, MaxPositionSize, MaxLeverage, VaR, Correlation, Concentration }
public enum RiskLevel { Normal, Elevated, Warning, Critical }
public enum MarketRegime { Trending, Ranging, Volatile, Crisis }
```

#### Domain Events

| Event | Trigger | Consumers |
|-------|---------|-----------|
| `PortfolioCreated` | New portfolio established | Coordination, Strategy |
| `PortfolioStatusChanged` | Active ↔ Paused → Closed | Execution, Strategy, Coordination |
| `CapitalReallocated` | Capital distribution changed | Execution, Evaluation, Coordination |
| `RiskLimitBreached` | Any risk threshold exceeded | Execution (halt), Coordination (alert) |
| `RiskLimitAdjusted` | Adaptive limits recalculated | Execution, Coordination |
| `RiskAssessmentCompleted` | Periodic or event-triggered risk eval | Coordination, Evaluation |

#### Repository Interfaces

```csharp
public interface IPortfolioRepository
{
    Task<Portfolio?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<Portfolio>> GetAllAsync();
    Task<IReadOnlyList<Portfolio>> GetByStatusAsync(PortfolioStatus status);
    Task AddAsync(Portfolio portfolio);
    void Update(Portfolio portfolio);
}

public interface IRiskRepository
{
    Task<IReadOnlyList<RiskAssessment>> GetLatestByPortfolioAsync(Guid portfolioId);
    Task<RiskAssessment?> GetCurrentAsync(Guid portfolioId);
    Task AddAsync(RiskAssessment assessment);
}
```

#### Relationship to Existing `Portfolio` Entity

The existing `Portfolio` entity is **enriched** but stays in this context:

- Core fields preserved: `Id`, `Name`, `Description`, `Status`, `CreatedAt`, `UpdatedAt`, `TargetReturn`, `TargetSharpe`, `AllocatedCapital`
- New: `RiskLimits[]` (child entities for typed risk constraints)
- New: `AllocationRules[]` (how capital is distributed)
- New: `AdaptiveRiskParams` (dynamic risk configuration)
- New: `MarketRegime` (current market state affecting risk)
- `PortfolioStatus` enum preserved as-is

---

### 2.4 Trade Execution Context

**Responsibility**: Order management, trade execution, fill tracking, exchange connectivity, slippage monitoring.

**Agent mapping**: Trade Execution Agent

#### Aggregate Roots

| Aggregate Root | Description |
|----------------|-------------|
| `Trade` | The trade lifecycle — from signal to execution to close. Owns its own consistency (entry/exit, PnL calculation). |

**Note**: `Trade` is NOT a child of `Portfolio` or `Strategy` in the DDD sense. It references them by ID but manages its own lifecycle independently. This is a deliberate design choice: the Execution context doesn't need to enforce Portfolio invariants, only Trade invariants.

#### Entities

| Entity | Parent | Description |
|--------|--------|-------------|
| `Order` | Trade | Individual order (entry order, exit order, partial fills) |
| `Fill` | Order | Confirmed execution fill from exchange |
| `ExecutionLog` | Trade | Audit trail of execution decisions and timing |

#### Value Objects

| Value Object | Fields | Description |
|--------------|--------|-------------|
| `TradeIdentifier` | `StrategyId`, `PortfolioId`, `Symbol`, `EntryTime` | Composite identity for trade lineage |
| `OrderRequest` | `Symbol`, `Side`, `Quantity`, `OrderType`, `Price?` | Order to be submitted |
| `ExecutionResult` | `OrderId`, `Status`, `FilledQuantity`, `AvgFillPrice`, `Fees`, `Slippage` | Result of order submission |
| `SlippageMetrics` | `ExpectedPrice`, `ActualPrice`, `SlippagePercent`, `SlippageAmount` | Slippage measurement |
| `TradeLifecycle` | `SignalTime`, `OrderTime`, `FillTime`, `CloseTime` | Timestamps through the trade lifecycle |

#### Enums

```csharp
public enum TradeSide { Long, Short }
public enum TradeStatus { Pending, Open, PartiallyClosed, Closed, Cancelled }
public enum OrderType { Market, Limit, StopLoss, StopLimit, TrailingStop }
public enum OrderStatus { Pending, Submitted, PartiallyFilled, Filled, Cancelled, Rejected }
public enum ExecutionVenue { Binance, Coinbase, Kraken, Simulated }
```

#### Domain Events

| Event | Trigger | Consumers |
|-------|---------|-----------|
| `OrderSubmitted` | Order sent to exchange | Coordination (monitor) |
| `OrderFilled` | Complete fill received | Evaluation (track PnL) |
| `TradeOpened` | Entry order filled, position established | Risk (exposure update), Evaluation |
| `TradeClosed` | Exit order filled, position closed | Evaluation (compute returns), Risk (free capital) |
| `TradeCancelled` | Order cancelled before fill | Coordination (log) |
| `SlippageReported` | Significant slippage detected | Risk (adjust limits), Coordination (alert) |

#### Repository Interfaces

```csharp
public interface ITradeRepository
{
    Task<Trade?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<Trade>> GetByPortfolioIdAsync(Guid portfolioId);
    Task<IReadOnlyList<Trade>> GetByStrategyIdAsync(Guid strategyId);
    Task<IReadOnlyList<Trade>> GetOpenTradesAsync(Guid? portfolioId, Guid? strategyId);
    Task AddAsync(Trade trade);
    void Update(Trade trade);
}

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<Order>> GetByTradeIdAsync(Guid tradeId);
    Task AddAsync(Order order);
    void Update(Order order);
}

public interface IExecutionLogRepository
{
    Task AddAsync(ExecutionLog log);
    Task<IReadOnlyList<ExecutionLog>> GetByTradeIdAsync(Guid tradeId);
}
```

#### Relationship to Existing `Trade` Entity

The existing `Trade` entity is **refactored** into a richer aggregate:

- Core fields preserved: `Id`, `Symbol`, `Side`, `EntryTime`, `ExitTime`, `EntryPrice`, `ExitPrice`, `Quantity`, `PnL`, `PnLPercent`, `Fees`, `Slippage`, `Status`, `AgentReason`
- `StrategyId` and `PortfolioId` kept as references (not navigation properties)
- New: `Order[]` (child entities tracking individual orders)
- New: `ExecutionLog[]` (audit trail)
- New: `TradeLifecycle` value object (timestamps through lifecycle)
- `TradeStatus` extended: `Pending` and `PartiallyClosed` added
- New: `OrderType`, `OrderStatus`, `ExecutionVenue` enums
- **Removed**: `ITradeRepository.CalculatePortfolioImpactAsync` and `CalculateStrategyImpactAsync` — these belong in the Evaluation context

---

### 2.5 Evaluation & Feedback Context

**Responsibility**: Performance measurement, reporting, benchmarking, feedback loops, strategy evaluation.

**Agent mapping**: Evaluation & Feedback Agent

#### Aggregate Roots

| Aggregate Root | Description |
|----------------|-------------|
| `PerformanceReport` | Aggregated performance report for a portfolio or strategy over a period. Owns the snapshot data for that period. |

#### Entities

| Entity | Parent | Description |
|--------|--------|-------------|
| `PerformanceSnapshot` | PerformanceReport | Daily performance metrics (return, equity, drawdown, Sharpe) |
| `BenchmarkComparison` | PerformanceReport | Comparison against benchmark (buy-and-hold, index, etc.) |
| `StrategyEvaluation` | PerformanceReport | Evaluation score and recommendation for a strategy |
| `FeedbackEntry` | PerformanceReport | Specific feedback/recommendation generated by the system |

#### Value Objects

| Value Object | Fields | Description |
|--------------|--------|-------------|
| `ReturnMetrics` | `TotalReturn`, `AnnualizedReturn`, `MonthlyReturns[]`, `DailyReturns[]` | Return measurements |
| `RiskMetrics` | `SharpeRatio`, `SortinoRatio`, `MaxDrawdown`, `CalmarRatio`, `Volatility`, `VaR` | Risk measurements |
| `TradeMetrics` | `TotalTrades`, `WinRate`, `ProfitFactor`, `AvgWin`, `AvgLoss`, `Expectancy` | Trade-level statistics |
| `ConfidenceInterval` | `Lower`, `Upper`, `ConfidenceLevel` | Statistical confidence range |
| `BenchmarkResult` | `BenchmarkReturn`, `Alpha`, `Beta`, `InformationRatio`, `TrackingError` | Benchmark comparison metrics |
| `EvaluationScore` | `Overall`, `Consistency`, `RiskAdjusted`, `Trend` (improving/stable/declining) | Composite evaluation |

#### Enums

```csharp
public enum EvaluationPeriod { Daily, Weekly, Monthly, Quarterly, Yearly, Custom }
public enum BenchmarkType { BuyAndHold, EqualWeight, RiskFreeRate, Custom }
public enum FeedbackType { Positive, Warning, Critical, Recommendation }
```

#### Domain Events

| Event | Trigger | Consumers |
|-------|---------|-----------|
| `PerformanceEvaluated` | Performance report generated | Coordination (dashboard), Strategy (adjust) |
| `StrategyScored` | Strategy evaluation completed | Strategy (rank), Risk (adjust allocation), Coordination |
| `DrawdownAlert` | Drawdown exceeds threshold | Risk (halt trading), Coordination (alert) |
| `FeedbackGenerated` | System generates improvement suggestion | Strategy (adapt), Coordination (log) |

#### Repository Interfaces

```csharp
public interface IPerformanceReportRepository
{
    Task<PerformanceReport?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<PerformanceReport>> GetByPortfolioAsync(Guid portfolioId, DateTime from, DateTime to);
    Task<PerformanceReport?> GetLatestAsync(Guid portfolioId);
    Task AddAsync(PerformanceReport report);
}

public interface IStrategyEvaluationRepository
{
    Task<IReadOnlyList<StrategyEvaluation>> GetByStrategyAsync(Guid strategyId, int count);
    Task<StrategyEvaluation?> GetLatestAsync(Guid strategyId);
    Task AddAsync(StrategyEvaluation evaluation);
}
```

#### Relationship to Existing `PerformanceSnapshot` Entity

The existing `PerformanceSnapshot` is **absorbed** into this context:

- All fields preserved: `Id`, `PortfolioId`, `StrategyId`, `Date`, `ActualReturn`, `SupposedReturn`, `DailyPnL`, `Equity`, `Drawdown`, `Sharpe`
- Now a child entity of `PerformanceReport` aggregate
- New: `BenchmarkComparison` for benchmark-relative analysis
- New: `StrategyEvaluation` for scoring strategies
- New: `FeedbackEntry` for system-generated recommendations
- New: `ReturnMetrics`, `RiskMetrics`, `TradeMetrics` value objects for structured metrics

---

### 2.6 Coordination Context

**Responsibility**: Agent orchestration, task scheduling, system health monitoring, inter-agent communication routing.

**Agent mapping**: Meta-Agent

#### Aggregate Roots

| Aggregate Root | Description |
|----------------|-------------|
| `AgentTask` | A unit of work assigned to an agent. Tracks lifecycle from creation to completion. Ensures task consistency. |
| `SystemHealth` | Aggregate for monitoring overall platform health and agent status. |

#### Entities

| Entity | Parent | Description |
|--------|--------|-------------|
| `AgentState` | SystemHealth | Current state of a specific agent (healthy, degraded, offline) |
| `TaskDependency` | AgentTask | Dependency between tasks (task B requires task A) |
| `CoordinationCycle` | SystemHealth | A single cycle of the autonomous loop (gather → decide → execute → evaluate) |

#### Value Objects

| Value Object | Fields | Description |
|--------------|--------|-------------|
| `AgentId` | `Value` (string), `AgentType` | Unique agent identifier |
| `TaskPriority` | `Value` (1–10), `IsUrgent` | Task priority with urgency flag |
| `CycleMetrics` | `Duration`, `AgentsParticipated`, `DecisionsMade`, `TradesExecuted` | Metrics for a coordination cycle |
| `HealthStatus` | `Overall`, `AgentStatuses[]`, `LastCheck` | System health snapshot |

#### Enums

```csharp
public enum AgentType { Meta, MarketIntelligence, StrategyDesign, RiskPortfolio, TradeExecution, EvaluationFeedback }
public enum AgentStatus { Healthy, Degraded, Offline, Initializing }
public enum TaskStatus { Pending, Assigned, InProgress, Completed, Failed, Cancelled }
public enum CyclePhase { DataGathering, Analysis, DecisionMaking, Execution, Evaluation }
```

#### Domain Events

| Event | Trigger | Consumers |
|-------|---------|-----------|
| `CycleStarted` | New coordination cycle begins | All agents |
| `CycleCompleted` | Coordination cycle finished | Dashboard, logging |
| `AgentStatusChanged` | Agent health state changes | Coordination (retry/failover) |
| `TaskAssigned` | Work item assigned to agent | Target agent |
| `TaskCompleted` | Agent finishes assigned work | Coordination (next step) |
| `SystemAlert` | Critical system condition detected | Dashboard, external alerting |

#### Repository Interfaces

```csharp
public interface IAgentTaskRepository
{
    Task<AgentTask?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<AgentTask>> GetByStatusAsync(TaskStatus status);
    Task<IReadOnlyList<AgentTask>> GetByAgentAsync(AgentType agent);
    Task AddAsync(AgentTask task);
    void Update(AgentTask task);
}

public interface ISystemHealthRepository
{
    Task<SystemHealth?> GetCurrentAsync();
    Task AddAsync(SystemHealth health);
    Task<IReadOnlyList<AgentState>> GetAgentStatesAsync();
}
```

---

## 3. Shared Kernel

The SharedKernel contains types used across multiple bounded contexts. It has zero dependencies on any context.

### Location

```
src/Certus.Domain/SharedKernel/
```

### Contents

| Type | Kind | Used By |
|------|------|---------|
| `IDomainEvent` | Interface | All contexts (events) |
| `AggregateRoot` | Abstract base class | All aggregate roots |
| `ValueObject` | Abstract base class | All value objects |
| `Entity` | Abstract base class | All entities |
| `Money` | Value object | Portfolio, Execution, Evaluation |
| `Symbol` | Value object | Market, Strategy, Execution |
| `DateRange` | Value object | Evaluation, Application queries |
| `PaginatedResult<T>` | Generic record | Application layer (cross-cutting) |
| `AssetClass` | Enum | Market, Strategy |
| `Currency` | Enum | Money |

### Code

```csharp
// SharedKernel/IDomainEvent.cs
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}

// SharedKernel/AggregateRoot.cs
public abstract class AggregateRoot : Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

// SharedKernel/Entity.cs
public abstract class Entity
{
    public Guid Id { get; protected set; }
}

// SharedKernel/ValueObject.cs (existing, preserved)

// SharedKernel/Money.cs
public record Money(decimal Amount, Currency Currency)
{
    public static Money Zero(Currency currency) => new(0m, currency);
    public static Money operator +(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException("Cannot add money with different currencies");
        return new Money(left.Amount + right.Amount, left.Currency);
    }
}

// SharedKernel/Symbol.cs
public record Symbol(string Value, AssetClass AssetClass)
{
    public override string ToString() => Value;
}

// SharedKernel/Currency.cs
public enum Currency { USD, EUR, BTC, ETH }
```

---

## 4. Aggregate Boundary Decisions

### Portfolio and Strategy: Separate Aggregates

**Decision**: `Portfolio` (Risk & Portfolio Context) and `StrategyDefinition` (Strategy Context) are **separate aggregate roots** in **separate bounded contexts**.

**Reasoning**:

1. **Different lifecycle**: A strategy can be created, backtested, and refined independently of any portfolio. A portfolio can exist with no strategies initially.

2. **Different consistency boundaries**: Portfolio enforces capital allocation consistency (total weight = 1.0, risk limits). Strategy enforces parameter/backtest consistency. These are different invariants.

3. **Autonomous agent alignment**: Each agent manages its own aggregate. The Risk agent manages Portfolio; the Strategy agent manages StrategyDefinition. Shared ownership would create coordination overhead.

4. **Future extraction**: When extracting to microservices, separate aggregates map cleanly to separate services with separate databases.

5. **Cross-reference pattern**: Strategy → Portfolio relationship is via `StrategySlot` (a value object or entity in the Strategy context that references `PortfolioId`). This avoids bidirectional navigation.

### Trade: Its Own Aggregate

**Decision**: `Trade` is an independent aggregate root in the Execution Context, referencing both `StrategyId` and `PortfolioId` by value (not navigation).

**Reasoning**: Trade has its own complex lifecycle (Pending → Open → PartiallyClosed → Closed → Cancelled) with its own invariants (entry before exit, PnL calculation). It doesn't need to be part of the Portfolio or Strategy aggregate.

### PerformanceSnapshot: Child of PerformanceReport

**Decision**: `PerformanceSnapshot` becomes a child entity within the `PerformanceReport` aggregate in the Evaluation Context.

**Reasoning**: Snapshots are always generated and consumed together as part of a report. They don't need independent identity or lifecycle.

---

## 5. Entity Migration Map

### Existing Entities

| Current Entity | Current Location | Destination | Action |
|----------------|------------------|-------------|--------|
| `Portfolio` | `Domain/Portfolios/` | `Domain/RiskAndPortfolio/Aggregates/Portfolio.cs` | **Move + Enrich** — add RiskLimits, AllocationRules, AdaptiveRiskParams |
| `Strategy` | `Domain/Strategies/` | `Domain/StrategyDesign/Aggregates/StrategyDefinition.cs` | **Move + Rename + Enrich** — add Parameters, BacktestRuns, Slots |
| `Trade` | `Domain/Trading/` | `Domain/Execution/Aggregates/Trade.cs` | **Move + Enrich** — add Orders[], ExecutionLogs[], Lifecycle |
| `PerformanceSnapshot` | `Domain/Trading/` | `Domain/Evaluation/Aggregates/PerformanceReport.cs` (as child) | **Move + Wrap** — becomes child entity of PerformanceReport |
| `PortfolioStatus` | `Domain/Portfolios/` | `Domain/RiskAndPortfolio/Enums/` | **Move** — preserved as-is |
| `StrategyStatus` | `Domain/Strategies/` | `Domain/StrategyDesign/Enums/` | **Move + Extend** — add `Draft`, `Backtesting` |
| `TradeStatus` | `Domain/Trading/` | `Domain/Execution/Enums/` | **Move + Extend** — add `Pending`, `PartiallyClosed` |
| `TradeSide` | `Domain/Trading/` | `Domain/Execution/Enums/` | **Move** — preserved as-is |
| `IDomainEvent` | `Domain/Events/` | `Domain/SharedKernel/IDomainEvent.cs` | **Move** — preserved as-is |
| `ValueObject` | `Domain/Shared/` | `Domain/SharedKernel/ValueObject.cs` | **Move** — preserved as-is |
| `DateRange` | `Domain/Shared/` | `Domain/SharedKernel/DateRange.cs` | **Move** — preserved as-is |
| `PaginatedResult<T>` | `Domain/Shared/` | `Domain/SharedKernel/PaginatedResult.cs` | **Move** — preserved as-is |
| `PortfolioSortBy` | `Domain/Shared/` | `Domain/Application/Shared/PortfolioSortBy.cs` | **Move to Application** — presentation concern |

### New Entities (to create)

| Entity | Context | Description |
|--------|---------|-------------|
| `RiskLimit` | Risk & Portfolio | Typed risk constraint per portfolio |
| `AllocationRule` | Risk & Portfolio | Capital distribution rules |
| `RiskAssessment` | Risk & Portfolio | Point-in-time risk evaluation |
| `CapitalAllocation` | Risk & Portfolio | Current allocation snapshot |
| `StrategyParameter` | Strategy Design | Configurable strategy parameter |
| `BacktestRun` | Strategy Design | Backtest execution record |
| `BacktestTrade` | Strategy Design | Trade within a backtest |
| `StrategySlot` | Strategy Design | Portfolio assignment with weight |
| `Order` | Execution | Individual order within a trade |
| `Fill` | Execution | Confirmed exchange fill |
| `ExecutionLog` | Execution | Audit trail |
| `PerformanceReport` | Evaluation | Aggregated performance report |
| `BenchmarkComparison` | Evaluation | Benchmark comparison |
| `StrategyEvaluation` | Evaluation | Strategy scoring |
| `FeedbackEntry` | Evaluation | System feedback |
| `AgentTask` | Coordination | Work item for agents |
| `AgentState` | Coordination | Agent health state |
| `CoordinationCycle` | Coordination | Autonomous loop cycle |
| `SystemHealth` | Coordination | System health aggregate |

### New Value Objects (to create)

| Value Object | Context |
|--------------|---------|
| `Money` | SharedKernel |
| `Symbol` | SharedKernel |
| `SignalStrength` | Market |
| `TimeFrame` | Market |
| `PriceRange` | Market |
| `VolumeProfile` | Market |
| `StrategyType` | Strategy Design |
| `BacktestConfig` | Strategy Design |
| `BacktestMetrics` | Strategy Design |
| `Weight` | Strategy Design |
| `RiskProfile` | Strategy Design |
| `ConfidenceInterval` | Strategy Design / Evaluation |
| `DrawdownLimit` | Risk & Portfolio |
| `PositionLimit` | Risk & Portfolio |
| `VaRMetric` | Risk & Portfolio |
| `AllocationTarget` | Risk & Portfolio |
| `AdaptiveRiskParams` | Risk & Portfolio |
| `OrderRequest` | Execution |
| `ExecutionResult` | Execution |
| `SlippageMetrics` | Execution |
| `TradeLifecycle` | Execution |
| `ReturnMetrics` | Evaluation |
| `RiskMetrics` | Evaluation |
| `TradeMetrics` | Evaluation |
| `BenchmarkResult` | Evaluation |
| `EvaluationScore` | Evaluation |
| `AgentId` | Coordination |
| `TaskPriority` | Coordination |
| `CycleMetrics` | Coordination |

---

## 6. Domain Events for Cross-Context Communication

### Phase 1 (Monolith): In-Process Events

In the monolith, domain events are dispatched synchronously via a local `IDomainEventDispatcher`:

```csharp
// SharedKernel/IDomainEventDispatcher.cs
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IDomainEvent domainEvent);
}

// Infrastructure/Events/InProcessDomainEventDispatcher.cs
public class InProcessDomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public InProcessDomainEventDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task DispatchAsync(IDomainEvent domainEvent)
    {
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
        var handlers = _serviceProvider.GetServices(handlerType);

        foreach (var handler in handlers)
        {
            var method = handlerType.GetMethod("HandleAsync");
            if (method != null)
            {
                await (Task)method.Invoke(handler, [domainEvent])!;
            }
        }
    }
}
```

### Phase 2 (Extracted Services): Message Bus

Events become messages on a bus (RabbitMQ, Azure Service Bus, or similar). The `IDomainEventDispatcher` implementation swaps to a message bus publisher. Event classes implement a serialization contract.

### Event Catalog

| Event | Source Context | Target Context(s) | Payload |
|-------|---------------|-------------------|---------|
| `MarketDataReceived` | Market | Strategy, Evaluation | Symbol, TimeFrame, BarCount |
| `SignalPublished` | Market | Strategy, Coordination | Symbol, Direction, Strength, IndicatorType |
| `AnomalyDetected` | Market | Risk, Coordination | Symbol, AnomalyType, Severity |
| `StrategyCreated` | Strategy | Coordination | StrategyId, Name, Type |
| `BacktestCompleted` | Strategy | Evaluation, Coordination | StrategyId, RunId, Metrics |
| `StrategyActivated` | Strategy | Risk, Execution, Coordination | StrategyId, PortfolioId, Weight |
| `StrategyDeactivated` | Strategy | Execution, Risk, Coordination | StrategyId, Reason |
| `StrategyParametersUpdated` | Strategy | Execution, Coordination | StrategyId, ChangedParams[] |
| `WeightAdjusted` | Strategy | Risk, Coordination | StrategyId, PortfolioId, OldWeight, NewWeight |
| `PortfolioCreated` | Risk & Portfolio | Coordination, Strategy | PortfolioId, Name, Capital |
| `PortfolioStatusChanged` | Risk & Portfolio | Execution, Strategy, Coordination | PortfolioId, OldStatus, NewStatus |
| `CapitalReallocated` | Risk & Portfolio | Execution, Evaluation, Coordination | PortfolioId, Allocations[] |
| `RiskLimitBreached` | Risk & Portfolio | Execution, Coordination | PortfolioId, LimitType, Severity |
| `RiskLimitAdjusted` | Risk & Portfolio | Execution, Coordination | PortfolioId, LimitType, OldValue, NewValue |
| `RiskAssessmentCompleted` | Risk & Portfolio | Coordination, Evaluation | PortfolioId, RiskLevel, Metrics |
| `OrderSubmitted` | Execution | Coordination | TradeId, OrderId, Symbol, Side |
| `OrderFilled` | Execution | Evaluation | TradeId, OrderId, FillQuantity, AvgPrice |
| `TradeOpened` | Execution | Risk, Evaluation | TradeId, PortfolioId, StrategyId, Symbol, Side, Size |
| `TradeClosed` | Execution | Evaluation, Risk | TradeId, PnL, Duration |
| `TradeCancelled` | Execution | Coordination | TradeId, Reason |
| `SlippageReported` | Execution | Risk, Coordination | TradeId, ExpectedPrice, ActualPrice |
| `PerformanceEvaluated` | Evaluation | Coordination, Strategy | ReportId, PortfolioId, Metrics |
| `StrategyScored` | Evaluation | Strategy, Risk, Coordination | StrategyId, Score, Recommendation |
| `DrawdownAlert` | Evaluation | Risk, Coordination | PortfolioId, Drawdown, Threshold |
| `FeedbackGenerated` | Evaluation | Strategy, Coordination | StrategyId, FeedbackType, Message |
| `CycleStarted` | Coordination | All | CycleId, Phase |
| `CycleCompleted` | Coordination | Dashboard | CycleId, Duration, Summary |
| `AgentStatusChanged` | Coordination | Coordination | AgentId, OldStatus, NewStatus |
| `TaskAssigned` | Coordination | Target Agent | TaskId, AgentType, Description |
| `TaskCompleted` | Coordination | Coordination | TaskId, Result |
| `SystemAlert` | Coordination | Dashboard, External | AlertType, Severity, Message |

---

## 7. Target Folder Structure (Phase 1 — Monolith)

```
src/
├── Certus.Domain/
│   ├── Certus.Domain.csproj
│   │
│   ├── SharedKernel/                          # Cross-cutting domain primitives
│   │   ├── AggregateRoot.cs                    # NEW — base class with domain events
│   │   ├── Entity.cs                           # NEW — base class with Id
│   │   ├── ValueObject.cs                      # MOVED from Shared/
│   │   ├── IDomainEvent.cs                     # MOVED from Events/
│   │   ├── IDomainEventHandler.cs              # NEW
│   │   ├── IDomainEventDispatcher.cs           # NEW
│   │   ├── Money.cs                            # NEW
│   │   ├── Symbol.cs                           # NEW
│   │   ├── Currency.cs                         # NEW
│   │   ├── AssetClass.cs                       # NEW
│   │   ├── DateRange.cs                        # MOVED from Shared/
│   │   └── PaginatedResult.cs                  # MOVED from Shared/
│   │
│   ├── Market/                                 # Market Intelligence Context
│   │   ├── Aggregates/
│   │   │   ├── MarketDataSeries.cs             # NEW — aggregate root
│   │   │   └── Signal.cs                       # NEW — entity
│   │   ├── ValueObjects/
│   │   │   ├── PriceBar.cs                     # NEW
│   │   │   ├── TechnicalIndicator.cs           # NEW
│   │   │   ├── SignalStrength.cs               # NEW
│   │   │   ├── TimeFrame.cs                    # NEW
│   │   │   ├── PriceRange.cs                   # NEW
│   │   │   └── VolumeProfile.cs                # NEW
│   │   ├── Enums/
│   │   │   ├── IndicatorType.cs                # NEW
│   │   │   ├── SignalDirection.cs              # NEW
│   │   │   └── FundamentalFactorType.cs        # NEW
│   │   ├── Events/
│   │   │   ├── MarketDataReceived.cs           # NEW
│   │   │   ├── SignalPublished.cs              # NEW
│   │   │   └── AnomalyDetected.cs              # NEW
│   │   └── Repositories/
│   │       ├── IMarketDataRepository.cs        # NEW
│   │       └── ISignalRepository.cs            # NEW
│   │
│   ├── Strategy/                               # Strategy Design & Simulation Context
│   │   ├── Aggregates/
│   │   │   └── StrategyDefinition.cs           # RENAMED+ENRICHED from Strategy.cs
│   │   ├── Entities/
│   │   │   ├── StrategyParameter.cs            # NEW
│   │   │   ├── BacktestRun.cs                  # NEW
│   │   │   ├── BacktestTrade.cs                # NEW
│   │   │   └── StrategySlot.cs                 # NEW
│   │   ├── ValueObjects/
│   │   │   ├── StrategyType.cs                 # NEW (replaces string Type)
│   │   │   ├── BacktestConfig.cs               # NEW
│   │   │   ├── BacktestMetrics.cs              # NEW
│   │   │   ├── Weight.cs                       # NEW (typed, validated)
│   │   │   ├── RiskProfile.cs                  # NEW
│   │   │   └── ConfidenceInterval.cs           # NEW
│   │   ├── Enums/
│   │   │   └── StrategyStatus.cs               # MOVED + EXTENDED
│   │   ├── Events/
│   │   │   ├── StrategyCreated.cs              # NEW
│   │   │   ├── BacktestCompleted.cs            # NEW
│   │   │   ├── StrategyActivated.cs            # NEW
│   │   │   ├── StrategyDeactivated.cs          # NEW
│   │   │   ├── StrategyParametersUpdated.cs    # NEW
│   │   │   └── WeightAdjusted.cs               # NEW
│   │   └── Repositories/
│   │       ├── IStrategyDefinitionRepository.cs # NEW (replaces IStrategyRepository)
│   │       └── IBacktestRepository.cs          # NEW
│   │
│   ├── RiskAndPortfolio/                       # Risk & Portfolio Context
│   │   ├── Aggregates/
│   │   │   └── Portfolio.cs                    # MOVED + ENRICHED from Portfolios/
│   │   ├── Entities/
│   │   │   ├── RiskLimit.cs                    # NEW
│   │   │   ├── AllocationRule.cs               # NEW
│   │   │   ├── RiskAssessment.cs               # NEW
│   │   │   └── CapitalAllocation.cs            # NEW
│   │   ├── ValueObjects/
│   │   │   ├── RiskLimitValue.cs               # NEW
│   │   │   ├── DrawdownLimit.cs                # NEW
│   │   │   ├── PositionLimit.cs                # NEW
│   │   │   ├── VaRMetric.cs                    # NEW
│   │   │   ├── AllocationTarget.cs             # NEW
│   │   │   └── AdaptiveRiskParams.cs           # NEW
│   │   ├── Enums/
│   │   │   ├── PortfolioStatus.cs              # MOVED from Portfolios/
│   │   │   ├── RiskLimitType.cs                # NEW
│   │   │   ├── RiskLevel.cs                    # NEW
│   │   │   └── MarketRegime.cs                 # NEW
│   │   ├── Events/
│   │   │   ├── PortfolioCreated.cs             # NEW
│   │   │   ├── PortfolioStatusChanged.cs       # NEW
│   │   │   ├── CapitalReallocated.cs           # NEW
│   │   │   ├── RiskLimitBreached.cs            # NEW
│   │   │   ├── RiskLimitAdjusted.cs            # NEW
│   │   │   └── RiskAssessmentCompleted.cs      # NEW
│   │   └── Repositories/
│   │       ├── IPortfolioRepository.cs         # MOVED from Portfolios/
│   │       └── IRiskRepository.cs              # NEW
│   │
│   ├── Execution/                              # Trade Execution Context
│   │   ├── Aggregates/
│   │   │   └── Trade.cs                        # MOVED + ENRICHED from Trading/
│   │   ├── Entities/
│   │   │   ├── Order.cs                        # NEW
│   │   │   ├── Fill.cs                         # NEW
│   │   │   └── ExecutionLog.cs                 # NEW
│   │   ├── ValueObjects/
│   │   │   ├── TradeIdentifier.cs              # NEW
│   │   │   ├── OrderRequest.cs                 # NEW
│   │   │   ├── ExecutionResult.cs              # NEW
│   │   │   ├── SlippageMetrics.cs              # NEW
│   │   │   └── TradeLifecycle.cs               # NEW
│   │   ├── Enums/
│   │   │   ├── TradeSide.cs                    # MOVED from Trading/
│   │   │   ├── TradeStatus.cs                  # MOVED + EXTENDED
│   │   │   ├── OrderType.cs                    # NEW
│   │   │   ├── OrderStatus.cs                  # NEW
│   │   │   └── ExecutionVenue.cs               # NEW
│   │   ├── Events/
│   │   │   ├── OrderSubmitted.cs               # NEW
│   │   │   ├── OrderFilled.cs                  # NEW
│   │   │   ├── TradeOpened.cs                  # NEW
│   │   │   ├── TradeClosed.cs                  # NEW
│   │   │   ├── TradeCancelled.cs               # NEW
│   │   │   └── SlippageReported.cs             # NEW
│   │   └── Repositories/
│   │       ├── ITradeRepository.cs             # MOVED + SIMPLIFIED
│   │       ├── IOrderRepository.cs             # NEW
│   │       └── IExecutionLogRepository.cs      # NEW
│   │
│   ├── Evaluation/                             # Evaluation & Feedback Context
│   │   ├── Aggregates/
│   │   │   └── PerformanceReport.cs            # NEW — wraps PerformanceSnapshot
│   │   ├── Entities/
│   │   │   ├── PerformanceSnapshot.cs          # MOVED from Trading/
│   │   │   ├── BenchmarkComparison.cs          # NEW
│   │   │   ├── StrategyEvaluation.cs           # NEW
│   │   │   └── FeedbackEntry.cs                # NEW
│   │   ├── ValueObjects/
│   │   │   ├── ReturnMetrics.cs                # NEW
│   │   │   ├── RiskMetrics.cs                  # NEW
│   │   │   ├── TradeMetrics.cs                 # NEW
│   │   │   ├── BenchmarkResult.cs              # NEW
│   │   │   └── EvaluationScore.cs              # NEW
│   │   ├── Enums/
│   │   │   ├── EvaluationPeriod.cs             # NEW
│   │   │   ├── BenchmarkType.cs                # NEW
│   │   │   └── FeedbackType.cs                 # NEW
│   │   ├── Events/
│   │   │   ├── PerformanceEvaluated.cs         # NEW
│   │   │   ├── StrategyScored.cs               # NEW
│   │   │   ├── DrawdownAlert.cs                # NEW
│   │   │   └── FeedbackGenerated.cs            # NEW
│   │   └── Repositories/
│   │       ├── IPerformanceReportRepository.cs # NEW
│   │       └── IStrategyEvaluationRepository.cs # NEW
│   │
│   └── Coordination/                           # Meta-Agent / Coordination Context
│       ├── Aggregates/
│       │   ├── AgentTask.cs                    # NEW
│       │   └── SystemHealth.cs                 # NEW
│       ├── Entities/
│       │   ├── AgentState.cs                   # NEW
│       │   ├── TaskDependency.cs               # NEW
│       │   └── CoordinationCycle.cs            # NEW
│       ├── ValueObjects/
│       │   ├── AgentId.cs                      # NEW
│       │   ├── TaskPriority.cs                 # NEW
│       │   ├── CycleMetrics.cs                 # NEW
│       │   └── HealthStatus.cs                 # NEW
│       ├── Enums/
│       │   ├── AgentType.cs                    # NEW
│       │   ├── AgentStatus.cs                  # NEW
│       │   ├── TaskStatus.cs                   # NEW
│       │   └── CyclePhase.cs                   # NEW
│       ├── Events/
│       │   ├── CycleStarted.cs                 # NEW
│       │   ├── CycleCompleted.cs               # NEW
│       │   ├── AgentStatusChanged.cs           # NEW
│       │   ├── TaskAssigned.cs                 # NEW
│       │   ├── TaskCompleted.cs                # NEW
│       │   └── SystemAlert.cs                  # NEW
│       └── Repositories/
│           ├── IAgentTaskRepository.cs         # NEW
│           └── ISystemHealthRepository.cs      # NEW
│
├── Certus.Application/
│   ├── Certus.Application.csproj
│   ├── Shared/
│   │   ├── PortfolioSortBy.cs                  # MOVED from Domain/Shared/
│   │   └── IEventBus.cs                        # NEW — application-level event bus interface
│   ├── Market/
│   │   ├── UseCases/
│   │   │   ├── IFetchMarketDataUseCase.cs      # NEW
│   │   │   ├── IGenerateSignalsUseCase.cs      # NEW
│   │   │   ├── FetchMarketDataUseCase.cs       # NEW
│   │   │   └── GenerateSignalsUseCase.cs       # NEW
│   │   └── DTOs/
│   │       ├── MarketDataDto.cs                # NEW
│   │       └── SignalDto.cs                    # NEW
│   ├── Strategy/
│   │   ├── UseCases/
│   │   │   ├── IManageStrategyUseCase.cs       # NEW
│   │   │   ├── IRunBacktestUseCase.cs          # NEW
│   │   │   ├── IActivateStrategyUseCase.cs     # NEW
│   │   │   ├── ManageStrategyUseCase.cs        # NEW
│   │   │   ├── RunBacktestUseCase.cs           # NEW
│   │   │   └── ActivateStrategyUseCase.cs      # NEW
│   │   └── DTOs/
│   │       ├── StrategyDefinitionDto.cs        # NEW
│   │       ├── BacktestRequestDto.cs           # NEW
│   │       ├── BacktestResultDto.cs            # NEW
│   │       ├── CreateStrategyRequest.cs        # MOVED from Strategies/DTOs/
│   │       ├── UpdateStrategyRequest.cs        # MOVED from Strategies/DTOs/
│   │       └── StrategyPerformanceDto.cs       # MOVED from Strategies/DTOs/
│   ├── Portfolio/
│   │   ├── UseCases/
│   │   │   ├── IManagePortfolioUseCase.cs      # NEW
│   │   │   ├── IAllocateCapitalUseCase.cs      # NEW
│   │   │   ├── IAssessRiskUseCase.cs           # NEW
│   │   │   ├── ManagePortfolioUseCase.cs       # NEW
│   │   │   ├── AllocateCapitalUseCase.cs       # NEW
│   │   │   └── AssessRiskUseCase.cs            # NEW
│   │   └── DTOs/
│   │       ├── PortfolioDto.cs                 # MOVED from Portfolios/DTOs/
│   │       ├── PortfolioDetailDto.cs           # MOVED
│   │       ├── DashboardKpis.cs                # MOVED
│   │       ├── PortfolioFilter.cs              # MOVED
│   │       ├── CreatePortfolioRequest.cs       # MOVED
│   │       ├── UpdatePortfolioRequest.cs       # MOVED
│   │       ├── RiskAssessmentDto.cs            # NEW
│   │       └── AllocationDto.cs                # NEW
│   ├── Execution/
│   │   ├── UseCases/
│   │   │   ├── IExecuteTradeUseCase.cs         # NEW
│   │   │   ├── IMonitorTradesUseCase.cs        # NEW
│   │   │   ├── ExecuteTradeUseCase.cs          # NEW
│   │   │   └── MonitorTradesUseCase.cs         # NEW
│   │   └── DTOs/
│   │       ├── TradeDto.cs                     # MOVED from Trading/DTOs/
│   │       ├── TradeDetailDto.cs               # MOVED
│   │       ├── TradeFilter.cs                  # MOVED
│   │       ├── OrderDto.cs                     # NEW
│   │       └── ExecutionReportDto.cs           # NEW
│   ├── Evaluation/
│   │   ├── UseCases/
│   │   │   ├── IGenerateReportUseCase.cs       # NEW
│   │   │   ├── IEvaluateStrategyUseCase.cs     # NEW
│   │   │   ├── GenerateReportUseCase.cs        # NEW
│   │   │   └── EvaluateStrategyUseCase.cs      # NEW
│   │   └── DTOs/
│   │       ├── PerformanceSnapshotDto.cs       # MOVED from Trading/DTOs/
│   │       ├── MonthlyReturnDto.cs             # MOVED
│   │       ├── PerformanceReportDto.cs         # NEW
│   │       ├── StrategyEvaluationDto.cs        # NEW
│   │       └── BenchmarkDto.cs                 # NEW
│   └── Coordination/
│       ├── UseCases/
│       │   ├── IOrchestrateCycleUseCase.cs     # NEW
│       │   ├── IMonitorSystemHealthUseCase.cs  # NEW
│       │   ├── OrchestrateCycleUseCase.cs      # NEW
│       │   └── MonitorSystemHealthUseCase.cs   # NEW
│       └── DTOs/
│           ├── CycleStatusDto.cs               # NEW
│           ├── AgentStatusDto.cs               # NEW
│           └── SystemHealthDto.cs              # NEW
│
├── Certus.Infrastructure/
│   ├── Certus.Infrastructure.csproj
│   ├── DependencyInjection.cs                 # EXTENDED — register all repos + event dispatcher
│   ├── Events/
│   │   ├── InProcessDomainEventDispatcher.cs   # NEW
│   │   └── DomainEventHandlers/               # NEW — event handler registrations
│   │       ├── StrategyActivatedHandler.cs     # NEW
│   │       ├── TradeClosedHandler.cs           # NEW
│   │       └── ...
│   └── Persistence/
│       ├── CertusDbContext.cs                  # EXTENDED — add new DbSets
│       ├── Configurations/
│       │   ├── Market/
│       │   │   ├── MarketDataSeriesConfiguration.cs  # NEW
│       │   │   └── SignalConfiguration.cs            # NEW
│       │   ├── Strategy/
│       │   │   ├── StrategyDefinitionConfiguration.cs # NEW (replaces StrategyConfiguration)
│       │   │   ├── BacktestRunConfiguration.cs        # NEW
│       │   │   └── StrategyParameterConfiguration.cs  # NEW
│       │   ├── RiskAndPortfolio/
│       │   │   ├── PortfolioConfiguration.cs          # MOVED + EXTENDED
│       │   │   ├── RiskLimitConfiguration.cs          # NEW
│       │   │   └── RiskAssessmentConfiguration.cs     # NEW
│       │   ├── Execution/
│       │   │   ├── TradeConfiguration.cs              # MOVED + EXTENDED
│       │   │   ├── OrderConfiguration.cs              # NEW
│       │   │   └── FillConfiguration.cs               # NEW
│       │   ├── Evaluation/
│       │   │   ├── PerformanceReportConfiguration.cs  # NEW
│       │   │   ├── PerformanceSnapshotConfiguration.cs # MOVED
│       │   │   └── StrategyEvaluationConfiguration.cs  # NEW
│       │   └── Coordination/
│       │       ├── AgentTaskConfiguration.cs          # NEW
│       │       └── SystemHealthConfiguration.cs       # NEW
│       ├── Repositories/
│       │   ├── Market/
│       │   │   ├── MarketDataRepository.cs    # NEW
│       │   │   └── SignalRepository.cs        # NEW
│       │   ├── Strategy/
│       │   │   ├── StrategyDefinitionRepository.cs # NEW (replaces StrategyRepository)
│       │   │   └── BacktestRepository.cs      # NEW
│       │   ├── RiskAndPortfolio/
│       │   │   ├── PortfolioRepository.cs     # MOVED from Repositories/
│       │   │   └── RiskRepository.cs          # NEW
│       │   ├── Execution/
│       │   │   ├── TradeRepository.cs         # MOVED + REFACTORED
│       │   │   ├── OrderRepository.cs         # NEW
│       │   │   └── ExecutionLogRepository.cs  # NEW
│       │   ├── Evaluation/
│       │   │   ├── PerformanceReportRepository.cs  # NEW
│       │   │   └── StrategyEvaluationRepository.cs # NEW
│       │   └── Coordination/
│       │       ├── AgentTaskRepository.cs     # NEW
│       │       └── SystemHealthRepository.cs  # NEW
│       └── SeedData.cs                        # EXTENDED — seed new entities
│
├── Certus.Dashboard/
│   ├── Certus.Dashboard.csproj
│   ├── Program.cs                             # EXTENDED — register new services
│   └── Components/                            # Existing Blazor components
│
└── Certus.Shared/                             # NEW — cross-cutting application concerns
    └── Certus.Shared.csproj                   # (Optional — only if needed beyond Domain/SharedKernel)
```

---

## 8. Cross-Context Communication Pattern

### Phase 1: In-Process Domain Events

```
┌─────────────────────────────────────────────────┐
│                  Monolith Process                 │
│                                                   │
│  Context A ──raises──▶ IDomainEvent               │
│                           │                       │
│                           ▼                       │
│              InProcessDomainEventDispatcher        │
│                           │                       │
│                           ▼                       │
│              IDomainEventHandler<T>               │
│              (registered in DI)                   │
│                           │                       │
│                           ▼                       │
│  Context B ──reacts──▶ business logic             │
│                                                   │
└─────────────────────────────────────────────────┘
```

**Pattern**:
1. Aggregate raises event via `RaiseDomainEvent()`
2. Repository saves entity + events to DB (or dispatches immediately)
3. Dispatcher resolves all `IDomainEventHandler<T>` from DI
4. Handlers execute synchronously within the same transaction (Phase 1)
5. Events cleared after dispatch

**Transactional safety**: In Phase 1, handlers run within the same DbContext transaction. If a handler fails, the entire operation rolls back. This is appropriate for a monolith.

### Phase 2: Out-of-Process Message Bus

```
┌──────────┐     ┌─────────────┐     ┌──────────┐
│ Context A │────▶│ Message Bus │────▶│ Context B │
│ (publish) │     │ (RabbitMQ)  │     │ (consume) │
└──────────┘     └─────────────┘     └──────────┘
```

**Pattern**:
1. Context A publishes event to message bus after commit
2. Message bus delivers to subscribed contexts
3. Context B consumes event, processes in its own transaction
4. Failure in B doesn't affect A (eventual consistency)

**Migration path**: Replace `InProcessDomainEventDispatcher` with a message bus publisher. Handlers become message consumers. No changes to event classes or handler interfaces.

---

## 9. DbContext Strategy

### Phase 1: Single Shared DbContext

One `CertusDbContext` with DbSets for all entities across all contexts. Folder-based isolation prevents cross-context DB access in application code.

```csharp
public class CertusDbContext : DbContext
{
    // Market Context
    public DbSet<MarketDataSeries> MarketDataSeries => Set<MarketDataSeries>();
    public DbSet<Signal> Signals => Set<Signal>();

    // Strategy Context
    public DbSet<StrategyDefinition> StrategyDefinitions => Set<StrategyDefinition>();
    public DbSet<BacktestRun> BacktestRuns => Set<BacktestRun>();

    // Risk & Portfolio Context
    public DbSet<Portfolio> Portfolios => Set<Portfolio>();
    public DbSet<RiskAssessment> RiskAssessments => Set<RiskAssessment>();

    // Execution Context
    public DbSet<Trade> Trades => Set<Trade>();
    public DbSet<Order> Orders => Set<Order>();

    // Evaluation Context
    public DbSet<PerformanceReport> PerformanceReports => Set<PerformanceReport>();
    public DbSet<PerformanceSnapshot> PerformanceSnapshots => Set<PerformanceSnapshot>();

    // Coordination Context
    public DbSet<AgentTask> AgentTasks => Set<AgentTask>();
    public DbSet<SystemHealth> SystemHealth => Set<SystemHealth>();
}
```

### Phase 2: Separate DbContext per Context

Each bounded context gets its own DbContext. Cross-context data access goes through repository interfaces (which become API clients in microservices).

```
Infrastructure/
├── Market/
│   └── MarketDbContext.cs
├── Strategy/
│   └── StrategyDbContext.cs
├── RiskAndPortfolio/
│   └── PortfolioDbContext.cs
├── Execution/
│   └── ExecutionDbContext.cs
├── Evaluation/
│   └── EvaluationDbContext.cs
└── Coordination/
    └── CoordinationDbContext.cs
```

---

## 10. Dependency Injection Registration

### Phase 1

```csharp
// Infrastructure/DependencyInjection.cs
public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
{
    // Single DbContext
    services.AddDbContext<CertusDbContext>(options =>
        options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

    // Event dispatcher
    services.AddScoped<IDomainEventDispatcher, InProcessDomainEventDispatcher>();

    // Market repositories
    services.AddScoped<IMarketDataRepository, MarketDataRepository>();
    services.AddScoped<ISignalRepository, SignalRepository>();

    // Strategy repositories
    services.AddScoped<IStrategyDefinitionRepository, StrategyDefinitionRepository>();
    services.AddScoped<IBacktestRepository, BacktestRepository>();

    // Risk & Portfolio repositories
    services.AddScoped<IPortfolioRepository, PortfolioRepository>();
    services.AddScoped<IRiskRepository, RiskRepository>();

    // Execution repositories
    services.AddScoped<ITradeRepository, TradeRepository>();
    services.AddScoped<IOrderRepository, OrderRepository>();
    services.AddScoped<IExecutionLogRepository, ExecutionLogRepository>();

    // Evaluation repositories
    services.AddScoped<IPerformanceReportRepository, PerformanceReportRepository>();
    services.AddScoped<IStrategyEvaluationRepository, StrategyEvaluationRepository>();

    // Coordination repositories
    services.AddScoped<IAgentTaskRepository, AgentTaskRepository>();
    services.AddScoped<ISystemHealthRepository, SystemHealthRepository>();

    // Event handlers
    services.AddScoped<IDomainEventHandler<StrategyActivated>, StrategyActivatedHandler>();
    services.AddScoped<IDomainEventHandler<TradeClosed>, TradeClosedHandler>();
    // ... register all handlers

    return services;
}
```

---

## 11. Implementation Phases

### Phase 1A: SharedKernel + Entity Refactoring (Week 1-2)

1. Create `SharedKernel/` folder with base classes (`AggregateRoot`, `Entity`, `ValueObject`)
2. Create `Money`, `Symbol`, `Currency`, `AssetClass` value objects
3. Refactor existing entities to inherit from `AggregateRoot` or `Entity`
4. Move existing shared types (`DateRange`, `PaginatedResult`, `IDomainEvent`)
5. Update existing tests

### Phase 1B: Context Scaffolding (Week 2-3)

1. Create all 6 context folders under `Domain/`
2. Move existing entities to their target contexts:
   - `Portfolio` → `RiskAndPortfolio/`
   - `Strategy` → `Strategy/` (rename to `StrategyDefinition`)
   - `Trade` → `Execution/`
   - `PerformanceSnapshot` → `Evaluation/`
3. Update namespaces throughout
4. Update all references in Application and Infrastructure layers
5. Ensure all tests pass

### Phase 1C: New Entities & Value Objects (Week 3-5)

1. Implement new value objects per context
2. Implement new entities (RiskLimit, Order, BacktestRun, etc.)
3. Implement new aggregate roots (PerformanceReport, AgentTask, SystemHealth)
4. Add domain events for each context
5. Write domain tests for all new types

### Phase 1D: Application Layer (Week 5-7)

1. Create UseCase interfaces and implementations per context
2. Create DTOs per context
3. Implement domain event handlers for cross-context communication
4. Create `InProcessDomainEventDispatcher`
5. Update `DependencyInjection.cs`
6. Write application layer tests

### Phase 1E: Infrastructure Layer (Week 7-9)

1. Create EF configurations for all new entities
2. Create repository implementations for all new interfaces
3. Extend `CertusDbContext` with new DbSets
4. Update seed data
5. Write integration tests

### Phase 1F: Dashboard + Integration (Week 9-11)

1. Update Dashboard components for new service interfaces
2. Add coordination dashboard (agent health, cycle status)
3. End-to-end testing
4. Performance testing

### Phase 2: DDD Extraction (Month 3-6)

1. Separate DbContexts per context
2. Implement message bus for cross-context events
3. Extract each context to its own service
4. Replace repository interfaces with API clients
5. Deploy as independent services

---

## 12. Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Namespace migration breaks all tests | High | Do Phase 1B in one commit, run full test suite |
| Cross-context DB access creeps in | Medium | Architecture tests enforce context boundaries |
| Event handler ordering issues | Medium | Phase 1 uses sync dispatch; add ordering in Phase 2 |
| Performance regression from added abstraction | Low | Profile early; value objects are struct-equivalent |
| Over-engineering for monolith phase | Medium | Keep UseCases thin; only add what the 6 agents need |

---

## 13. Architecture Test Rules (New)

```csharp
// Add to Certus.ArchitectureTests
[Fact]
public void Contexts_Should_Not_Cross_Reference_Directly()
{
    // Market context should not reference Strategy context types directly
    // (communication only via domain events)
    var result = Types.InAssembly(domainAssembly)
        .That()
        .ResideInNamespace("Certus.Domain.Market")
        .Should()
        .NotHaveDependencyOn("Certus.Domain.Strategy")
        .GetResult();

    Assert.True(result.IsSuccessful);
}

[Fact]
public void SharedKernel_Should_Not_Reference_Any_Context()
{
    var result = Types.InAssembly(domainAssembly)
        .That()
        .ResideInNamespace("Certus.Domain.SharedKernel")
        .Should()
        .NotHaveDependencyOnAny(
            "Certus.Domain.Market",
            "Certus.Domain.Strategy",
            "Certus.Domain.RiskAndPortfolio",
            "Certus.Domain.Execution",
            "Certus.Domain.Evaluation",
            "Certus.Domain.Coordination")
        .GetResult();

    Assert.True(result.IsSuccessful);
}
```
