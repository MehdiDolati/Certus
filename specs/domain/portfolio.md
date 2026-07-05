# Portfolio Entity

## Status
Active

## Overview
Portfolio is the top-level aggregation in the Certus trading platform. It groups strategies, trades, and performance snapshots.

## Data Model

```
Portfolio
  Id               : Guid              // Unique identifier
  Name             : string            // Display name (required, max 200 chars)
  Description      : string            // Description (max 1000 chars)
  Status           : PortfolioStatus   // Active | Paused | Closed
  CreatedAt        : DateTime          // Creation timestamp
  UpdatedAt        : DateTime          // Last update timestamp
  TargetReturn     : decimal           // Annual target return %
  TargetSharpe     : decimal           // Target Sharpe ratio
  AllocatedCapital : decimal           // USD amount allocated
```

## Status Values

| Value | Description |
|-------|-------------|
| Active | Portfolio is operational, can accept strategies |
| Paused | Temporarily suspended, strategies exist but no new trades |
| Closed | Permanently closed, no modifications allowed |

## Business Rules

1. **BR-001**: A portfolio can only add strategies when status is Active
2. **BR-002**: Status transitions: Active → Paused, Paused → Active, Active → Closed, Paused → Closed
3. **BR-003**: Closed portfolios cannot be reopened
4. **BR-004**: AllocatedCapital must be non-negative
5. **BR-005**: TargetReturn represents annual percentage (e.g., 0.25 = 25%)

## Computed Properties

| Property | Formula | Description |
|----------|---------|-------------|
| IsActive | Status == Active | Whether portfolio is operational |
| CanAddStrategy | Status == Active | Whether new strategies can be added |
| AllocatedCapitalInMillions | AllocatedCapital / 1,000,000 | Capital in millions |

## Acceptance Criteria

- [ ] AC-001: Given a portfolio with Active status, when checking IsActive, then it returns true
- [ ] AC-002: Given a portfolio with Paused status, when checking IsActive, then it returns false
- [ ] AC-003: Given a portfolio with Active status, when calling CanAddStrategy, then it returns true
- [ ] AC-004: Given a portfolio with Closed status, when calling CanAddStrategy, then it returns false
- [ ] AC-005: Given a portfolio with AllocatedCapital of 12,400,000, when accessing AllocatedCapitalInMillions, then it returns 12.4

## Test Mapping

| Acceptance Criteria | Test File | Test Method |
|---------------------|-----------|-------------|
| AC-001 | PortfolioTests.cs | Portfolio_IsActive_Should_Return_True_When_Status_Is_Active |
| AC-002 | PortfolioTests.cs | Portfolio_IsActive_Should_Return_False_When_Status_Is_Not_Active |
| AC-003 | PortfolioTests.cs | Portfolio_CanAddStrategy_Should_Return_True_Only_When_Active |
| AC-004 | PortfolioTests.cs | Portfolio_CanAddStrategy_Should_Return_True_Only_When_Active |
| AC-005 | PortfolioTests.cs | Portfolio_AllocatedCapitalInMillions_Should_Divide_By_One_Million |
