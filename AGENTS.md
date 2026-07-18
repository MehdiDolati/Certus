# AGENTS.md

## What this is

Certus — autonomous multi-agent trading platform. C# .NET 10, Clean Architecture, Blazor Server, SQL Server, spec-driven development.

## Build & test commands

```bash
dotnet restore
dotnet build --configuration Release --no-restore

# Run all tests (CI order — respect this sequence)
dotnet test tests/Certus.Domain.Tests/
dotnet test tests/Certus.Application.Tests/
dotnet test tests/Certus.Infrastructure.Tests/
dotnet test tests/Certus.IntegrationTests/
dotnet test tests/Certus.ArchitectureTests/

# Run a single test project
dotnet test tests/Certus.Domain.Tests/ --filter "FullyQualifiedName~Portfolio"

# Run a single test
dotnet test tests/Certus.Domain.Tests/ --filter "TestMethod_Name"

# Docker (SQL Server + Dashboard)
docker compose up --build
# Dashboard at http://localhost:8080
```

## Architecture (enforced by NetArchTest)

```
Domain ──────> (nothing)
Application ─> Domain
Infrastructure -> Application, Domain
Dashboard ───> Application (+ Infrastructure for DI wiring)
```

Dependency rules are **non-negotiable** — architecture tests will fail the build if violated:
1. Domain must NOT reference Application, Infrastructure, or Dashboard
2. Application must NOT reference Infrastructure or Dashboard
3. Infrastructure may reference Application and Domain
4. Dashboard references Application (services) and Infrastructure (DI)
5. Bounded contexts must NOT reference each other directly (only via domain events)
6. SharedKernel must NOT reference any bounded context

## Solution structure

- **Solution file**: `Certus.slnx` (XML format, not `.sln`)
- **Entry point**: `src/Certus.Dashboard/` (Blazor Server, the only runnable project)
- **6 bounded contexts** in `src/Certus.Domain/`: Market, Strategy, RiskAndPortfolio, Execution, Evaluation, Coordination
- **SharedKernel**: `src/Certus.Domain/SharedKernel/` — Entity, AggregateRoot, IDomainEvent, Money VO, Symbol VO
- **Repository pattern**: interfaces in Domain, implementations in Infrastructure
- **DI wiring**: `src/Certus.Infrastructure/DependencyInjection.cs` + `src/Certus.Dashboard/Program.cs`

## Test coverage targets

- **Domain, Application, Infrastructure**: 100% required
- **Architecture tests**: 100% (NetArchTest rules)
- **Integration, Component, E2E**: Best effort
- **CI only runs**: Domain, Application, Infrastructure, Integration, Architecture (NOT Component, NOT E2E)
- Framework: xUnit + FluentAssertions + Moq + NetArchTest

## Spec-driven development workflow

**No spec, no code. No tests, no merge.**

1. Read/create spec in `specs/` (domain rules, features, architecture)
2. Write tests from acceptance criteria (AC-001, AC-002, ...)
3. Implement to make tests pass
4. Update spec if requirements change

Specs live in `specs/domain/`, `specs/features/`, `specs/architecture/`. Full index at `SPEC.md`.

## Known gotchas

- **Blazor Server DbContext threading**: `Task.WhenAll` on services sharing a scoped `CertusDbContext` throws "A second operation was started". Always use sequential `await` calls.
- **EF Core + SQLite in-memory tests**: Must share a single `SqliteConnection` across test lifetime. `DataSource=:memory:` creates a new DB per connection.
- **EF Core identity-map conflicts in tests**: Call `ChangeTracker.Clear()` after service calls when test and service share the same scoped DbContext.
- **Test environment skip**: `Program.cs` skips migrations/seeding when `ASPNETCORE_ENVIRONMENT=Testing`. Tests use `EnsureCreated` with SQLite.
- **bUnit version**: Must use bUnit 1.35.x for MudBlazor 7 compatibility (not 2.7.x).
- **Test naming convention**: `Method_Scenario_ExpectedResult` (e.g., `Portfolio_CanAddStrategy_Should_Return_False_When_Closed`).
- **Cross-platform path handling**: On Linux (CI), backslash is NOT a directory separator. Never convert `/` to `\` and then pass the result to `Path.GetDirectoryName`, `Path.Combine`, or `Directory.Exists` — they will fail silently or throw `ArgumentNullException`. Only normalize for string matching, not filesystem ops.

## Key files to read before implementing

- `specs/README.md` — spec-driven workflow and enforcement checklist
- `ARCHITECTURE_PLAN.md` — layer rules, bounded contexts, test architecture
- `SPEC.md` — full spec index (17 specs)
- `specs/architecture/clean-architecture.md` — dependency rules detail
- `specs/architecture/domain-architecture.md` — 6 bounded contexts, aggregate roots
