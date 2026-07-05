# Certus — Architecture Reference

## Layer Structure

```
Domain ──────> (nothing)
Application ─> Domain
Infrastructure -> Application, Domain
Dashboard ───> Application (+ Infrastructure for DI wiring)
```

## Bounded Contexts (6)

```
┌─────────────────────────────────────────────────────────────────┐
│  Market          Strategy         Execution                      │
│  Intelligence ──▶ Design &   ──▶ Trade                          │
│                  Simulation      Execution                       │
│       │              │               │                           │
│       ▼              ▼               ▼                           │
│  Risk &          Evaluation       Coordination                   │
│  Portfolio    ◀── & Feedback      (Meta-Agent)                   │
└─────────────────────────────────────────────────────────────────┘
                     SharedKernel
```

### Context → Aggregate Root Mapping

| Context | Aggregate Root | Key Entities | Agent |
|---------|---------------|--------------|-------|
| Market | MarketDataSeries | Signal, PriceBar, TechnicalIndicator | Market Intelligence |
| Strategy | StrategyDefinition | StrategyParameter, BacktestRun, StrategySlot | Strategy Design |
| RiskAndPortfolio | Portfolio | RiskLimit, AllocationRule, RiskAssessment | Risk & Portfolio |
| Execution | Trade | Order, Fill, ExecutionLog | Trade Execution |
| Evaluation | PerformanceReport | PerformanceSnapshot, BenchmarkComparison, StrategyEvaluation | Evaluation & Feedback |
| Coordination | AgentTask | SystemHealth, AgentState, CoordinationCycle | Meta-Agent |

## Dependency Rules (Enforced by NetArchTest)

1. Domain must NOT reference Application, Infrastructure, or Dashboard
2. Application must NOT reference Infrastructure or Dashboard
3. Infrastructure may reference Application and Domain
4. Dashboard references Application (for services) and Infrastructure (for DI)
5. Bounded contexts must NOT reference each other directly (only via domain events)
6. SharedKernel must NOT reference any bounded context

## Repository Pattern

- Interfaces live in Domain (within each context's Repositories/ folder)
- Implementations live in Infrastructure (within each context's Repositories/ folder)
- Services inject interfaces, not DbContext

## Domain Events

Cross-context communication uses in-process domain events:
- Aggregate raises event via `RaiseDomainEvent()`
- `InProcessDomainEventDispatcher` resolves handlers from DI
- Handlers execute synchronously within the same transaction (Phase 1)
- Phase 2: swap to message bus (RabbitMQ/Azure Service Bus)

## CRUD Operations

| Entity | Create | Read | Update | Delete |
|--------|--------|------|--------|--------|
| Portfolio | CreateAsync | GetAll, GetDetail | UpdateAsync | DeleteAsync (soft) |
| StrategyDefinition | CreateAsync | GetByPortfolio | UpdateAsync | DeleteAsync (soft) |
| Trade | (via Execution) | GetFiltered | (via Execution) | (via Execution) |

Delete is always soft-delete (status change to Closed/Retired).

## Validation Rules

- Portfolio name: required, max 200 chars
- Strategy weight: 0.0 - 1.0 (via Weight value object)
- Cannot add strategy to non-Active portfolio
- Trade entry before exit enforced

## Test Architecture

| Layer | Test Type | Framework | What it Tests |
|-------|-----------|-----------|---------------|
| Domain | Unit | xUnit + FluentAssertions | Entity behavior, value objects |
| Application | Unit | xUnit + Moq + FluentAssertions | Service logic with mocked repos |
| Integration | Integration | xUnit + WebApplicationFactory | Full pipeline with real SQLite |
| Architecture | Architecture | xUnit + NetArchTest | Dependency rules, context boundaries |
| E2E | E2E | xUnit + Playwright | Browser-based UI flows |
