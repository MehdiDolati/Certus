<!--
Sync Impact Report
- Version change: (template, no content) -> 1.0.0
- Modified principles: All placeholders populated from repo governance (AGENTS.md, README, ARCHITECTURE_PLAN)
- Added sections: Core Principles (5), Architecture Constraints, Development Workflow, Governance, RATIFICATION_DATE/LAST_AMENDED_DATE
- Removed sections: none
- Follow-up TODOs: RATIFICATION_DATE set to 2026-09-09 (initial adoption date; no prior constitution existed)
-->

# Certus Constitution

## Core Principles

### I. Spec-Driven Development (NON-NEGOTIABLE)

**No spec, no code. No tests, no merge.**

Every feature MUST start with a specification in `specs/` covering domain rules, features, and
architecture. Tests MUST be written from the spec's acceptance criteria (AC-001, AC-002, ...) and
MUST be approved before implementation begins. Implementation exists solely to make tests pass.
When requirements change, the spec MUST be updated first, then tests, then implementation.

### II. Clean Architecture & Dependency Rules (NON-NEGOTIABLE)

The dependency graph MUST be enforced by NetArchTest on every build:

```
Domain ──────> (nothing)
Application ─> Domain
Infrastructure -> Application, Domain
Dashboard ───> Application (+ Infrastructure for DI wiring)
```

Bounded contexts MUST NOT reference each other directly; they communicate only via domain events.
SharedKernel MUST NOT reference any bounded context. Violations MUST fail the build. This rule
cannot be relaxed by convention, refactor, or technical debt allowance.

### III. Research Before Trading

Every trading strategy MUST start as a documented hypothesis. A hypothesis MUST be documented,
tested, validated, and challenged before it may become a candidate for deployment. Evidence MUST
prevail over assumptions. Successful and failed experiments are both accumulated knowledge assets;
failed experiments MUST be recorded with the same rigor as successful ones.

### IV. Test-First with 100% Coverage Gates

Domain, Application, and Infrastructure test projects MUST achieve 100% coverage. Architecture
tests MUST cover 100% of NetArchTest rules. The red-green-refactor cycle MUST be strictly
enforced: write a failing test, verify it fails for the expected reason, then implement the minimum
to make it pass. Coverage targets are release-blocking, not aspirational.

### V. Progressive Autonomy & Transparency

Certus evolves from human-supervised workflows toward higher AI autonomy, but MUST progress
incrementally and only as trust is earned. Initial phases MUST prioritize human decision-making,
transparent workflows, explainable results, and controlled experimentation. Black-box trading
systems and strategy optimization based only on historical backtests MUST NOT be introduced.

## Architecture Constraints

- The system MAY only be extended inside the six bounded contexts: Market, Strategy,
  RiskAndPortfolio, Execution, Evaluation, Coordination.
- Repository interfaces MUST live in Domain; implementations MUST live in Infrastructure.
- DI wiring MUST follow `src/Certus.Infrastructure/DependencyInjection.cs` plus the Dashboard
  `Program.cs` composition root—no service is registered ad hoc inside a page.
- Blazor Server pages MUST issue sequential (not parallel) awaits when sharing a scoped
  `CertusDbContext`; `Task.WhenAll` over such services is FORBIDDEN (throws "A second operation
  was started").
- EF Core + SQLite in-memory tests MUST share a single `SqliteConnection` across the test
  lifetime; `DataSource=:memory:` creates a fresh DB per connection and is FORBIDDEN.
- Tests sharing a scoped DbContext with the service under test MUST call `ChangeTracker.Clear()`
  after service calls to avoid identity-map conflicts.
- When `ASPNETCORE_ENVIRONMENT=Testing`, `Program.cs` MUST skip migrations/seeding; tests use
  `EnsureCreated` with SQLite.
- bUnit MUST remain on 1.35.x for MudBlazor 7 compatibility.
- Path handling MUST be cross-platform: on Linux, backslash is NOT a directory separator. Never
  convert `/` to `\` before `Path.GetDirectoryName`, `Path.Combine`, or `Directory.Exists`.

## Development Workflow

1. Read or create the spec in `specs/` (domain rules, features, architecture). Full index at
   `SPEC.md`.
2. Write tests from acceptance criteria before any implementation code.
3. Implement the minimum required to make the tests pass.
4. Update the spec if requirements change; a stale spec is a release-blocking defect.
5. Run the test suite in the CI order only:
   `dotnet test tests/Certus.Domain.Tests/` → `tests/Certus.Application.Tests/` →
   `tests/Certus.Infrastructure.Tests/` → `tests/Certus.IntegrationTests/` →
   `tests/Certus.ArchitectureTests/`.
6. Test methods MUST use the naming convention `Method_Scenario_ExpectedResult`
   (e.g., `Portfolio_CanAddStrategy_Should_Return_False_When_Closed`).
7. Integration, component, and E2E tests are best-effort in CI (component/E2E are NOT run in CI).

## Governance

This Constitution supersedes all other practices, documented or implicit, where they conflict.
Amendments MUST be documented in this file with the new version, an approval step, and a migration
plan for existing work. Version bumps MUST follow semantic versioning:

- **MAJOR**: backward-incompatible governance/principle removals or redefinitions.
- **MINOR**: new principle/section added or materially expanded guidance.
- **PATCH**: clarifications, wording, and non-semantic refinements.

Every PR and code review MUST verify compliance with this Constitution. Complexity MUST be
justified in the review; unexplained complexity is a rejection reason. Runtime development
guidance lives in `AGENTS.md` and `ARCHITECTURE_PLAN.md`; this Constitution is the binding
governance source.

**Version**: 1.0.0 | **Ratified**: 2026-09-09 | **Last Amended**: 2026-09-09