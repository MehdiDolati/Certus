# Portfolio & Strategy CRUD

## Status
Draft

## Overview
Add Create, Update, and Delete operations for Portfolios and Strategies. Delete is soft-delete (status change to Closed/Retired).

## Requirements

### Functional Requirements
- [ ] FR-001: Create a new portfolio with name, description, target return, target Sharpe, allocated capital
- [ ] FR-002: Update portfolio name, description, targets, and allocated capital
- [ ] FR-003: Soft-delete portfolio by setting status to Closed
- [ ] FR-004: Create a new strategy within a portfolio with name, type, target return, weight
- [ ] FR-005: Update strategy name, type, target return, weight
- [ ] FR-006: Soft-delete strategy by setting status to Retired

### Non-Functional Requirements
- [ ] NFR-001: All mutations validate input (name required, weight 0.0-1.0)
- [ ] NFR-002: Create returns the new entity with generated Id

## Business Rules

1. Portfolio name is required, max 200 chars
2. Strategy weight must be between 0.0 and 1.0
3. Delete is always soft-delete (status change, never physical removal)
4. Cannot create strategy on a non-Active portfolio

## Acceptance Criteria

### Portfolio
- [ ] AC-001: Given valid input, when creating a portfolio, then portfolio is returned with Id
- [ ] AC-002: Given an existing portfolio, when updating name, then name is persisted
- [ ] AC-003: Given an Active portfolio, when soft-deleting, then status becomes Closed
- [ ] AC-004: Given a portfolio with empty name, when creating, then validation error is thrown

### Strategy
- [ ] AC-005: Given a valid strategy input, when creating, then strategy is returned with Id
- [ ] AC-006: Given an existing strategy, when updating weight, then weight is persisted
- [ ] AC-007: Given an Active strategy, when soft-deleting, then status becomes Retired
- [ ] AC-008: Given weight of 1.5, when creating strategy, then validation error is thrown

## Test Mapping

| Acceptance Criteria | Test File | Test Method |
|---------------------|-----------|-------------|
| AC-001 | PortfolioCrudTests.cs | CreateAsync_Should_Return_Portfolio_With_Id |
| AC-002 | PortfolioCrudTests.cs | UpdateAsync_Should_Persist_Changes |
| AC-003 | PortfolioCrudTests.cs | DeleteAsync_Should_Set_Status_Closed |
| AC-004 | PortfolioCrudTests.cs | CreateAsync_Should_Throw_On_Empty_Name |
| AC-005 | StrategyCrudTests.cs | CreateAsync_Should_Return_Strategy_With_Id |
| AC-006 | StrategyCrudTests.cs | UpdateAsync_Should_Persist_Changes |
| AC-007 | StrategyCrudTests.cs | DeleteAsync_Should_Set_Status_Retired |
| AC-008 | StrategyCrudTests.cs | CreateAsync_Should_Throw_On_Invalid_Weight |
