# Module Boundaries

## Status
Active

## Overview
The application is organized into three feature modules: Portfolios, Strategies, and Trading. Each module is isolated to enable future extraction to microservices.

## Module Structure

Each module follows the same pattern:

```
Module/
├── IEntityRepository.cs    # Domain interface
├── IEntity.cs              # Domain entity
├── IEntityService.cs       # Application interface
├── EntityService.cs        # Application implementation
└── DTOs/                   # Data transfer objects
```

## Module Definitions

### Portfolios Module
- **Entities**: Portfolio
- **Services**: IPortfolioService, PortfolioService
- **DTOs**: PortfolioDto, PortfolioDetailDto, DashboardKpis, PortfolioFilter

### Strategies Module
- **Entities**: Strategy
- **Services**: IStrategyService, StrategyService
- **DTOs**: StrategyPerformanceDto

### Trading Module
- **Entities**: Trade, PerformanceSnapshot
- **Services**: ITradeService, TradeService
- **DTOs**: TradeDto, TradeDetailDto, PerformanceSnapshotDto, TradeFilter, MonthlyReturnDto

## Isolation Rules

1. Modules communicate through service interfaces, not direct DB access
2. Cross-module queries go through the repository layer
3. No module directly references another module's internal types
4. DTOs are module-specific (no shared DTO types between modules)

## Future Extraction

When extracting to microservices:
1. Each module becomes a separate service
2. Repository interfaces become API clients
3. Domain events enable async communication
4. Shared kernel (Domain/Shared) becomes a shared library
