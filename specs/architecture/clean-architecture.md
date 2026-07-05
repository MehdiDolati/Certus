# Clean Architecture Rules

## Status
Active

## Overview
This project follows Clean Architecture with strict dependency rules enforced by NetArchTest.

## Layer Structure

```
Domain ──────> (nothing)
Application ─> Domain
Infrastructure -> Application, Domain
Dashboard ───> Application, Infrastructure (DI wiring only)
```

## Dependency Rules

1. **Domain** must NOT reference Application, Infrastructure, or Dashboard
2. **Application** must NOT reference Infrastructure or Dashboard
3. **Infrastructure** may reference Application and Domain
4. **Dashboard** references Application (for services) and Infrastructure (for DI wiring)

## Domain Layer Rules

- Zero NuGet package references
- Entities are pure domain objects with behavior
- Repository interfaces live in Domain (abstractions only)
- Value objects override Equals/GetHashCode
- No static mutable state

## Application Layer Rules

- References only Domain
- Contains DTOs, service interfaces, and service implementations
- Services inject repository interfaces (NOT DbContext)
- No EF Core references
- No direct database access

## Infrastructure Layer Rules

- Implements repository interfaces from Domain
- Contains DbContext, EF configurations, seed data
- Provides DependencyInjection extension method
- All read queries use AsNoTracking()

## Dashboard Layer Rules

- Blazor Server components
- References Application for service interfaces
- References Infrastructure for DI wiring in Program.cs
- Components consume services via interfaces

## Architecture Tests

NetArchTest rules enforce these constraints at build time:

| Test | Rule |
|------|------|
| Domain_Should_Not_Reference_Application | Domain has no Application dependency |
| Domain_Should_Not_Reference_Infrastructure | Domain has no Infrastructure dependency |
| Application_Should_Not_Reference_Infrastructure | Application has no Infrastructure dependency |
| Domain_Should_Not_Depend_On_EntityFramework | Domain has no EF Core dependency |
| Application_Should_Not_Depend_On_EntityFramework | Application has no EF Core dependency |
| Infrastructure_Should_Implement_Domain_Interfaces | Infrastructure references Domain |

## Verification

Run architecture tests:
```bash
dotnet test tests/Certus.ArchitectureTests/
```
