# Certus — Development Roadmap

## Project Status: Domain Architecture Restructured

Clean Architecture modular monolith with 6 bounded contexts, TDD, spec-driven development, and full test coverage.

---

## Architecture

```
Certus.slnx
├── src/
│   ├── Certus.Domain/              # Zero dependencies — 6 bounded contexts + SharedKernel
│   │   ├── SharedKernel/           # Base classes, Money, Symbol, IDomainEvent
│   │   ├── Market/                 # Market Intelligence Context
│   │   ├── Strategy/               # Strategy Design & Simulation Context
│   │   ├── RiskAndPortfolio/       # Risk & Portfolio Context
│   │   ├── Execution/              # Trade Execution Context
│   │   ├── Evaluation/             # Evaluation & Feedback Context
│   │   └── Coordination/           # Meta-Agent Context (placeholder)
│   ├── Certus.Application/         # Depends on Domain — services, DTOs
│   ├── Certus.Infrastructure/      # Depends on Application+Domain — EF Core, repos
│   └── Certus.Dashboard/           # Blazor Server UI
└── tests/
    ├── Certus.Domain.Tests/         # 26 unit tests
    ├── Certus.Application.Tests/    # 20 unit tests
    ├── Certus.IntegrationTests/     # 20 integration tests (real DB)
    ├── Certus.ComponentTests/       # bUnit component tests
    ├── Certus.ArchitectureTests/    # 6 NetArchTest rules
    └── Certus.E2ETests/             # Playwright E2E tests
```

---

## Completed Phases

### Phase 1: Clean Architecture Restructuring
- Split monolithic project into 4 layers (Domain, Application, Infrastructure, Dashboard)
- Enforced dependency rules via NetArchTest
- Repository pattern with interfaces in Domain, implementations in Infrastructure

### Phase 2: Domain Layer (Original)
- Entities with behavior: Portfolio, Strategy, Trade, PerformanceSnapshot
- Value objects: DateRange, PaginatedResult
- Repository interfaces: IPortfolioRepository, IStrategyRepository, ITradeRepository

### Phase 3: Application Layer
- Service interfaces and implementations: PortfolioService, StrategyService, TradeService
- DTOs: PortfolioDto, StrategyPerformanceDto, TradeDto, etc.
- CRUD operations: Create, Update, Delete for Portfolios and Strategies
- Validation: name required, weight 0.0-1.0, active portfolio for new strategies

### Phase 4: Infrastructure Layer
- EF Core DbContext with SQLite
- Repository implementations with AsNoTracking()
- Entity configurations (fluent API)
- Seed data: 5 portfolios, 12 strategies, ~158 trades, 1530 snapshots

### Phase 5: Dashboard UI
- Portfolio Dashboard: KPI cards, performance table, filters, sorting
- Portfolio Detail: equity curve, drawdown chart, monthly heatmap, strategy table, trade log
- CRUD dialogs: PortfolioFormDialog, StrategyFormDialog
- Action buttons: Create, Edit, Delete on tables
- Status-aware controls: "New Strategy" disabled for non-Active portfolios

### Phase 6: Testing Strategy
- **Unit tests**: Domain entities, Application services (mocked repos)
- **Integration tests**: Real service → repo → EF Core → SQLite in-memory
- **Architecture tests**: NetArchTest dependency rules
- **E2E tests**: Playwright browser automation for UI flows

### Phase 9: Platform Integration
- Plugin architecture for trading platform adapters
- MetaTrader 4 plugin with file-based data import
- FileImportService with FileSystemWatcher for real-time monitoring
- PlatformService with connect, disconnect, import operations
- 120 tests covering all 18 acceptance criteria

### Coverage Targets

| Layer | Target | Current | Rationale |
|-------|--------|---------|-----------|
| Domain | **100%** | 96.3% | All business logic, entities, value objects, events |
| Application | **100%** | 90.8% | All service methods, DTOs, request/response types |
| Infrastructure | **100%** | — | Repositories, file I/O, plugin loading |
| Architecture | 100% | 100% | NetArchTest dependency rules |
| Integration | Best effort | — | Full pipeline with real DB |
| E2E | Behavior only | — | Browser-based UI flows |

**Why 100% for Domain?** The Domain layer contains all business rules, invariants, and entity behavior. Missing coverage here means untested business logic — the highest-risk area for bugs.

**What can't be measured?** Blazor components (run in SignalR circuits), EF Core configurations (framework metadata), and startup/DI wiring (verified by integration tests).

### Phase 7: DevOps
- Dockerfile: Multi-stage build (SDK → runtime)
- docker-compose.yml: Single service with persistent SQLite volume
- .dockerignore: Excludes bin/obj, .git, test data

### Phase 8: Domain Architecture Restructuring (NEW)
- **SharedKernel**: Entity, AggregateRoot, IDomainEvent, Money, Symbol, AssetClass, Currency
- **6 Bounded Contexts** mapped to 6 autonomous agent modules:
  - **Market**: MarketDataSeries (root), Signal, PriceBar, TechnicalIndicator, SignalStrength
  - **Strategy**: StrategyDefinition (root), StrategyParameter, BacktestRun, StrategySlot, StrategyType
  - **RiskAndPortfolio**: Portfolio (root), RiskLimit, AllocationRule, RiskAssessment, AdaptiveRiskParams
  - **Execution**: Trade (root), Order, Fill, ExecutionLog, TradeLifecycle
  - **Evaluation**: PerformanceReport (root), PerformanceSnapshot, BenchmarkComparison, StrategyEvaluation
  - **Coordination**: AgentTask (root), SystemHealth, AgentState (lightweight placeholder)
- **Cross-context communication**: In-process domain events via IDomainEventDispatcher
- **30+ domain events** for inter-context integration
- **27+ value objects** for type-safe domain modeling
- All 74 tests passing (26 Domain + 20 Application + 6 Architecture + 20 Integration + 1 Component + 1 Infrastructure)

---

## Spec-Driven Development

Specs live in `specs/` directory:

```
specs/
├── README.md              # Workflow documentation
├── TEMPLATE.md            # Spec template
├── domain/                # Entity rules & invariants
│   ├── portfolio.md
│   ├── strategy.md
│   └── trading.md
├── features/              # Feature specifications
│   ├── portfolio-dashboard.md
│   ├── portfolio-detail.md
│   └── portfolio-crud.md
├── architecture/          # Architecture rules
│   ├── clean-architecture.md
│   ├── module-boundaries.md
│   └── domain-architecture.md   # NEW — 6 bounded contexts design
└── session-log.md         # Prompt tracking
```

---

## Development Workflow

### Branching: GitHub Flow

Simple branching strategy for solo development:

| Branch | Purpose | Example |
|--------|---------|---------|
| `main` | Production-ready, always deployable | — |
| `feature/*` | New features | `feature/market-data-ingestion` |
| `fix/*` | Bug fixes | `fix/portfolio-calculation-error` |
| `chore/*` | Maintenance, docs, config | `chore/update-pipeline` |

**Process**:
1. Create branch from `main`: `git checkout -b feature/market-data`
2. Commit often with clear messages
3. Push and open PR (CI runs automatically)
4. CI passes → merge to `main` → delete branch

### Commit Convention

```
type(scope): description

feat(market): add PriceBar entity and repository
fix(portfolio): correct weight calculation
test(domain): add StrategyDefinition invariant tests
chore(ci): update pipeline test step
```

Types: `feat`, `fix`, `test`, `chore`, `refactor`, `docs`

### Feature Management: GitHub Issues

- Issue templates in `.github/ISSUE_TEMPLATE/` for features, bugs, and chores
- Domain labels (`domain:market`, `domain:strategy`, etc.) for categorization
- Priority labels for triage
- Each feature issue links to its spec in `specs/features/`

### Issue Templates

| Template | Use For |
|----------|---------|
| `feature.md` | New features with acceptance criteria |
| `bug.md` | Bug reports with reproduction steps |
| `task.md` | Maintenance and chore work |

### PR Template

All PRs include:
- Description of changes
- Related issue reference
- Type of change (feature/fix/refactor/docs/chore)
- Checklist (tests, spec, docs)

### Release Strategy

Simple tag-based releases on `main`:

```bash
git tag -a v0.1.0 -m "Initial release: Portfolio CRUD"
git push origin v0.1.0
```

---

## How to Run

```bash
# Build
dotnet build Certus.slnx

# Run all tests
dotnet test Certus.slnx

# Run specific test projects
dotnet test tests/Certus.Domain.Tests/
dotnet test tests/Certus.Application.Tests/
dotnet test tests/Certus.IntegrationTests/

# Run the app
dotnet run --project src/Certus.Dashboard

# Docker
docker compose up --build
```

---

## Key Design Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Architecture | Clean Architecture + Bounded Contexts | Enforceable boundaries, testable, agent-aligned |
| Database | SQLite + EF Core | Simple for dev, swap to PostgreSQL for prod |
| UI Framework | MudBlazor | Rich component library, dark theme |
| Charts | Hand-drawn SVG | Zero JS interop, full control |
| Testing | xUnit + FluentAssertions | Readable assertions, good ecosystem |
| E2E Testing | Playwright | Cross-browser, reliable, modern |
| Spec Format | Markdown | Version-controlled, human-readable |
| Cross-context comms | In-process domain events | Swappable to message bus later |
| Aggregate roots | Portfolio, StrategyDefinition, Trade, PerformanceReport, AgentTask | Each owned by one agent, separate lifecycle |
| Source Control | GitHub Flow | Simple branching, ideal for solo dev |
| Feature Tracking | GitHub Issues + Labels | Built-in, lightweight, no external tools |
