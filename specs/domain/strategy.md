# Strategy Entity

## Status
Active

## Overview
A Strategy belongs to a Portfolio and represents a specific trading approach (e.g., Momentum, MeanReversion). Each strategy has its own performance tracking.

## Data Model

```
Strategy
  Id            : Guid            // Unique identifier
  PortfolioId   : Guid            // Parent portfolio
  Name          : string          // Display name (required, max 200 chars)
  Type          : string          // Strategy type (max 100 chars)
  Status        : StrategyStatus  // Active | Paused | Retired
  TargetReturn  : decimal         // Target annual return %
  Weight        : decimal         // Allocation weight within portfolio (0.0-1.0)
  StartedAt     : DateTime        // When strategy was activated
```

## Status Values

| Value | Description |
|-------|-------------|
| Active | Strategy is actively trading |
| Paused | Temporarily suspended |
| Retired | Permanently decommissioned |

## Business Rules

1. **BR-001**: Weight must be between 0.0 and 1.0
2. **BR-002**: Sum of weights for all strategies in a portfolio should equal 1.0
3. **BR-003**: Strategy type is a free-form string (e.g., "Momentum", "MeanReversion", "Arbitrage")

## Computed Properties

| Property | Formula | Description |
|----------|---------|-------------|
| IsActive | Status == Active | Whether strategy is operational |

## Acceptance Criteria

- [ ] AC-001: Given a strategy with Active status, when checking IsActive, then it returns true
- [ ] AC-002: Given a strategy with Retired status, when checking IsActive, then it returns false
- [ ] AC-003: Given a strategy with Weight 0.6, when accessing Weight, then it returns 0.6

## Test Mapping

| Acceptance Criteria | Test File | Test Method |
|---------------------|-----------|-------------|
| AC-001 | StrategyTests.cs | Strategy_IsActive_Should_Return_True_When_Status_Is_Active |
| AC-002 | StrategyTests.cs | Strategy_IsActive_Should_Return_False_When_Status_Is_Not_Active |
| AC-003 | StrategyTests.cs | Strategy_Should_Be_Created_With_Default_Values |
