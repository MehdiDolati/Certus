# Certus — Specification Index

This project follows **spec-driven development**. Individual specs live in the `specs/` directory.

## Quick Links

| Spec | Location | Description |
|------|----------|-------------|
| [Portfolio Entity](specs/domain/portfolio.md) | `specs/domain/` | Portfolio entity rules, invariants, status values |
| [Strategy Entity](specs/domain/strategy.md) | `specs/domain/` | Strategy entity rules, weight constraints |
| [Trade & PerformanceSnapshot](specs/domain/trading.md) | `specs/domain/` | Trade lifecycle, PnL calculation, snapshots |
| [Portfolio Dashboard](specs/features/portfolio-dashboard.md) | `specs/features/` | Dashboard page: KPIs, table, filters, sorting |
| [Portfolio Detail](specs/features/portfolio-detail.md) | `specs/features/` | Detail report: charts, strategies, trade log |
| [Clean Architecture](specs/architecture/clean-architecture.md) | `specs/architecture/` | Layer rules, dependency constraints, NetArchTest |
| [Module Boundaries](specs/architecture/module-boundaries.md) | `specs/architecture/` | Module isolation, future extraction |
| [Domain Architecture](specs/architecture/domain-architecture.md) | `specs/architecture/` | 6 bounded contexts, aggregate roots, domain events |

## Workflow

1. Read the relevant spec before implementing a feature
2. Write tests based on acceptance criteria
3. Implement to make tests pass
4. Update the spec if requirements change

See `specs/README.md` for the full spec-driven development workflow.
