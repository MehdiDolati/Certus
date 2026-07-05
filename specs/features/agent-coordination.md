# Agent Coordination

## Status
Active

## Overview
Agent Coordination manages task assignment, agent health monitoring, coordination cycles, and inter-agent communication for the autonomous trading platform.

## Requirements

### Functional Requirements
- [ ] FR-001: Create tasks assigned to specific agents with priority and description
- [ ] FR-002: Track task lifecycle (Pending → InProgress → Completed/Failed/Cancelled)
- [ ] FR-003: Add dependencies between tasks
- [ ] FR-004: Monitor agent health status (Healthy, Degraded, Offline, Initializing)
- [ ] FR-005: Track coordination cycles with metrics
- [ ] FR-006: Generate system alerts for critical conditions

### Non-Functional Requirements
- [ ] NFR-001: Task priorities must support urgency levels (1-10)
- [ ] NFR-002: Agent heartbeats must be tracked for health monitoring

## Data Model

### AgentTask (Aggregate Root)
```
AgentTask
  Id               : Guid
  AssignedAgent    : AgentType         // Meta | MarketIntelligence | StrategyDesign | RiskPortfolio | TradeExecution | EvaluationFeedback
  Description      : string
  Status           : TaskStatus        // Pending | Assigned | InProgress | Completed | Failed | Cancelled
  Priority         : TaskPriority      // Value (1-10), IsUrgent
  CreatedAt        : DateTime
  StartedAt        : DateTime?
  CompletedAt      : DateTime?
  Result           : string?
  ErrorMessage     : string?
  Dependencies     : TaskDependency[]
```

### SystemHealth (Aggregate Root)
```
SystemHealth
  Id               : Guid
  OverallStatus    : AgentStatus       // Derived from agent states
  AgentStates      : AgentState[]      // Current state of each agent
  Cycles           : CoordinationCycle[] // Recent coordination cycles
  LastChecked      : DateTime
```

### AgentState (Entity)
```
AgentState
  Id               : Guid
  AgentType        : AgentType
  Status           : AgentStatus       // Healthy | Degraded | Offline | Initializing
  Message          : string?
  LastHeartbeat    : DateTime
```

### CoordinationCycle (Entity)
```
CoordinationCycle
  Id               : Guid
  CycleNumber      : int
  Phase            : CyclePhase        // DataGathering | Analysis | DecisionMaking | Execution | Evaluation
  Metrics          : CycleMetrics
  StartedAt        : DateTime
  CompletedAt      : DateTime?
```

### TaskDependency (Entity)
```
TaskDependency
  Id               : Guid
  TaskId           : Guid              // Dependent task
  DependsOnTaskId  : Guid              // Required task
```

## Enums

| Enum | Values |
|------|--------|
| AgentType | Meta, MarketIntelligence, StrategyDesign, RiskPortfolio, TradeExecution, EvaluationFeedback |
| AgentStatus | Healthy, Degraded, Offline, Initializing |
| TaskStatus | Pending, Assigned, InProgress, Completed, Failed, Cancelled |
| CyclePhase | DataGathering, Analysis, DecisionMaking, Execution, Evaluation |

## Business Rules

1. **BR-001**: Task priority Value must be between 1 and 10
2. **BR-002**: AgentTask transitions: Pending → InProgress, InProgress → Completed/Failed, any → Cancelled
3. **BR-003**: SystemHealth.OverallStatus is derived from the worst AgentState status

## Acceptance Criteria

- [ ] AC-001: Given a task with Priority 10 and IsUrgent true, when checking priority, then it's Critical
- [ ] AC-002: Given a pending task, when starting, then Status becomes InProgress and StartedAt is set
- [ ] AC-003: Given an in-progress task, when completing with a result, then Status becomes Completed
- [ ] AC-004: Given an in-progress task, when failing with an error, then Status becomes Failed
- [ ] AC-005: Given a task, when adding a dependency, then Dependencies list contains the dependency
- [ ] AC-006: Given a SystemHealth with one Offline agent, when checking OverallStatus, then it's Offline

## Test Mapping

| Acceptance Criteria | Test File | Test Method |
|---------------------|-----------|-------------|
| AC-001 | TaskPriorityTests.cs | Critical_Should_Have_Value_10_And_IsUrgent |
| AC-002 | AgentTaskTests.cs | Start_Should_Set_Status_To_InProgress |
| AC-003 | AgentTaskTests.cs | Complete_Should_Set_Status_To_Completed |
| AC-004 | AgentTaskTests.cs | Fail_Should_Set_Status_To_Failed |
| AC-005 | AgentTaskTests.cs | AddDependency_Should_Add_To_List |
| AC-006 | SystemHealthTests.cs | OverallStatus_Should_Be_Worst_Agent_Status |

## Dependencies

- Domain: SharedKernel (AggregateRoot, IDomainEvent)
- Domain: Coordination.Enums (AgentType, AgentStatus, TaskStatus, CyclePhase)
- Domain: Coordination.ValueObjects (AgentId, TaskPriority, CycleMetrics, HealthStatus)
