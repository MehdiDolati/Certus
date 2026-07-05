# Spec-Driven Development

This project follows **spec-driven development** — specs are written BEFORE implementation.

## Workflow

```
Spec → Acceptance Criteria → Tests → Implementation → Refactor
```

1. **Write Spec**: Define requirements, data models, business rules, acceptance criteria
2. **Write Tests**: Convert acceptance criteria into test cases
3. **Implement**: Write code to make tests pass (TDD)
4. **Refactor**: Clean up while keeping tests green
5. **Verify**: Run `dotnet test` — all must pass

## Directory Structure

```
specs/
├── README.md           # This file
├── TEMPLATE.md         # Spec template for new features
├── domain/             # Domain entity rules & invariants
│   ├── portfolio.md
│   ├── strategy.md
│   └── trading.md
├── features/           # Feature specifications
│   ├── portfolio-dashboard.md
│   └── portfolio-detail.md
└── architecture/       # Architecture rules & constraints
    ├── clean-architecture.md
    └── module-boundaries.md
```

## Naming Conventions

- Spec files: `kebab-case.md`
- Acceptance criteria: `AC-001`, `AC-002`, ...
- Functional requirements: `FR-001`, `FR-002`, ...
- Non-functional requirements: `NFR-001`, `NFR-002`, ...

## How Specs Map to Tests

Each spec includes a **Test Mapping** table:

| Acceptance Criteria | Test File | Test Method |
|---------------------|-----------|-------------|
| AC-001 | PortfolioTests.cs | Should_Return_True_When_Active |

## Updating Specs

- When requirements change, update the spec FIRST
- Then update tests to match new acceptance criteria
- Then update implementation
- Specs are version-controlled alongside code

## Rules

- No feature implementation starts without a spec
- Every acceptance criterion must have at least one test
- Specs are reviewed before implementation begins
- **Domain layer must have 100% test coverage** — all entities, value objects, domain events, and repository interfaces
- **Application layer must have 100% test coverage** — all services, DTOs, and request/response types
- CI pipeline runs coverage checks; PRs that reduce coverage below target are blocked