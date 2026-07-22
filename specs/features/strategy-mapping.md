# Strategy Mapping

## Status
Active

## Overview
Links platform Expert Advisor external IDs (e.g., "EA_Momentum_01") to internal StrategyDefinition records. When trades are imported from a connected platform, the EA name on each trade is resolved to a Certus strategy via this mapping.

## Requirements

### Functional Requirements
- [ ] FR-001: A `StrategyMapping` entity links `(ConnectionId, StrategyExternalId)` to a `StrategyDefinition.Id`
- [ ] FR-002: Each mapping is unique per `(ConnectionId, StrategyExternalId)` — no duplicate mappings
- [ ] FR-003: When importing a trade, `FindOrCreateStrategyMappingAsync` looks up existing mapping or creates a new one
- [ ] FR-004: If no `StrategyDefinition` exists for the EA name, one is auto-created with type `ExpertAdvisor`
- [ ] FR-005: Existing trades with `StrategyId = Guid.Empty` can be re-linked after mapping is created

### Non-Functional Requirements
- [ ] NFR-001: Mapping lookup must be efficient — indexed on `(ConnectionId, StrategyExternalId)`
- [ ] NFR-002: Auto-created strategies use `StrategyType(Custom, "ExpertAdvisor")` to distinguish from manually created strategies

## Data Model

### StrategyMapping (Entity)
```
StrategyMapping
  Id                    : Guid                    // Unique identifier
  ConnectionId          : Guid                    // FK to PlatformConnection
  StrategyExternalId    : string                  // EA name from platform (e.g., "EA_Momentum_01")
  StrategyDefinitionId  : Guid                    // FK to StrategyDefinition
  CreatedAt             : DateTime                // When mapping was created
```

## Business Rules

1. A mapping is unique per `(ConnectionId, StrategyExternalId)` — creating a duplicate raises an error
2. `StrategyExternalId` is required and max 100 characters
3. When `strategyExternalId` is null or empty, `FindOrCreateStrategyMappingAsync` returns `Guid.Empty` (no mapping created)
4. Auto-created strategies have `StrategyType(StrategyCategory.Custom, "ExpertAdvisor")` and `TargetReturn = 0`

## Acceptance Criteria

- [ ] AC-001: Given a connection and EA name with no existing mapping, when `FindOrCreateStrategyMappingAsync` is called, then a new `StrategyDefinition` and `StrategyMapping` are created, and the strategy ID is returned
- [ ] AC-002: Given a connection and EA name with an existing mapping, when `FindOrCreateStrategyMappingAsync` is called, then the existing `StrategyDefinition.Id` is returned without creating duplicates
- [ ] AC-003: Given an empty `strategyExternalId`, when `FindOrCreateStrategyMappingAsync` is called, then `Guid.Empty` is returned
- [ ] AC-004: The `StrategyMapping` entity exposes `ConnectionId`, `StrategyExternalId`, `StrategyDefinitionId`, and `CreatedAt`
- [ ] AC-005: The `StrategyMapping` repository supports lookup by `(ConnectionId, StrategyExternalId)`

## Test Mapping

| Acceptance Criteria | Test File | Test Method |
|---------------------|-----------|-------------|
| AC-001 | PlatformServiceTests.cs | ImportTradesAsync_Should_Create_Strategy_And_Mapping_When_None_Exists |
| AC-002 | PlatformServiceTests.cs | ImportTradesAsync_Should_Use_Existing_Mapping_When_Found |
| AC-003 | PlatformServiceTests.cs | ImportTradesAsync_Should_Return_Empty_When_StrategyExternalId_Is_Empty |
| AC-004 | StrategyMappingTests.cs | Create_Should_Set_Properties_Correctly |
| AC-004 | StrategyMappingTests.cs | Create_Should_Set_CreatedAt_To_UtcNow |

## Dependencies

- `specs/features/platform-integration.md` — Platform connection and trade import
- `specs/domain/strategy.md` — StrategyDefinition entity
