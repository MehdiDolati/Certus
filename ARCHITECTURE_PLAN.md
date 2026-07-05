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

| Layer | Test Type | Framework | Coverage Target | What it Tests |
|-------|-----------|-----------|-----------------|---------------|
| Domain | Unit | xUnit + FluentAssertions | **100%** | Entity behavior, value objects, domain events |
| Application | Unit | xUnit + Moq + FluentAssertions | **100%** | Service logic with mocked repos |
| Infrastructure | Unit/Integration | xUnit + SQLite in-memory | **100%** | Repositories, file I/O, plugin loading |
| Architecture | Architecture | xUnit + NetArchTest | 100% | Dependency rules, context boundaries |
| Integration | Integration | xUnit + WebApplicationFactory | Best effort | Full pipeline with real DB |
| E2E | E2E | xUnit + Playwright | Behavior only | Browser-based UI flows |

### Coverage Policy

- **Domain layer**: 100% line and branch coverage required. This is where all business logic lives — every entity, value object, domain event, and repository interface must have tests.
- **Application layer**: 100% coverage required. All service methods, DTOs, and request/response types must be tested.
- **Infrastructure layer**: 100% coverage required for repositories, file services, and plugin loaders. EF Core configurations are covered by integration tests.
- **Architecture tests**: 100% — NetArchTest rules must all pass.
- **Integration/E2E**: Best-effort. Integration tests exercise the full pipeline; E2E tests verify behavior through a real browser.

### What Cannot Be Coverage-Measured

- **Blazor components**: Render inside SignalR circuits, outside the .NET process the coverage tool monitors. Use E2E tests for behavioral verification.
- **EF Core configurations**: Metadata consumed by the framework. Covered implicitly by integration tests that query the database.
- **Program.cs / DI wiring**: Startup code. Verified by integration tests spinning up the full app.

### Best Practices

1. **Test the behavior, not the implementation** — test what a class does, not how it does it internally.
2. **One assertion per concept** — each test should verify one logical behavior.
3. **Use descriptive test names** — `Method_Scenario_ExpectedResult` pattern (e.g., `Portfolio_CanAddStrategy_Should_Return_False_When_Closed`).
4. **Arrange-Act-Assert** — every test follows this structure with clear separation.
5. **Test domain events** — verify that entities raise the correct events with correct data.
6. **Test value object equality** — records should test structural equality, not reference equality.
7. **Test boundary conditions** — min/max values, empty inputs, null handling, edge cases.
8. **Test error paths** — invalid state transitions, validation failures, not-found scenarios.
9. **Keep tests independent** — no shared state between tests, no test ordering dependencies.
10. **Mock at boundaries** — mock repository interfaces in Application tests; use real SQLite in Infrastructure tests.
