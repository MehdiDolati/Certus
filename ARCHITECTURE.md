# Architecture Overview

This solution follows **Clean Architecture** principles with clear separation of concerns.

## Layer Responsibilities

### 🔷 Certus.Domain (Core Layer)
- **Purpose**: Contains business logic and domain entities
- **Dependencies**: None
- **Contents**:
  - Domain Entities (business objects)
  - Value Objects
  - Domain Interfaces
  - Domain Events
  - Business Rules & Validations

### 📦 Certus.Application (Application Services Layer)
- **Purpose**: Orchestrates business logic and coordinates between layers
- **Dependencies**: Domain
- **Contents**:
  - DTOs (Data Transfer Objects)
  - Application Services
  - Request/Response handlers
  - Mapping profiles
  - Application-level Exceptions
  - Interfaces for repositories and services

### 🔌 Certus.Infrastructure (External Services Layer)
- **Purpose**: Implements data access and external service integrations
- **Dependencies**: Domain, Application
- **Contents**:
  - Entity Framework Core DbContext
  - Repository implementations
  - Database migrations
  - External API clients
  - Authentication/Authorization implementations

### 🌐 Certus.API (Presentation Layer - REST API)
- **Purpose**: Exposes REST endpoints for client consumption
- **Dependencies**: Application, Infrastructure
- **Contents**:
  - Controllers
  - API Models (requests/responses)
  - Middleware
  - Startup configuration
  - Dependency injection setup

### 🎨 Certus.UI (Presentation Layer - Blazor)
- **Purpose**: Web UI using Blazor Web App
- **Dependencies**: Application
- **Contents**:
  - Blazor components
  - Pages
  - Services (API clients)
  - State management

### 🧪 Certus.Tests.Unit
- **Purpose**: Unit tests for individual components
- **Framework**: xUnit
- **Tools**: Moq, FluentAssertions

### 🔗 Certus.Tests.Integration
- **Purpose**: Integration tests across layers
- **Framework**: xUnit
- **Tools**: WebApplicationFactory, TestServer

## Dependency Flow

```
UI/API
  ↓
Application (Use Cases)
  ↓
Domain (Business Logic)
  ↑
Infrastructure (Data Access)
```

✅ **Inner layers are unaware of outer layers**
✅ **Dependencies point inward**
✅ **Easy to test (mock external dependencies)**

## File Organization Templates

### Domain Layer
```
Certus.Domain/
├── Common/
│   └── BaseEntity.cs
├── Entities/
│   └── YourEntity.cs
└── Interfaces/
    └── IRepository.cs
```

### Application Layer
```
Certus.Application/
├── DTOs/
│   └── YourEntityDto.cs
├── Services/
│   └── YourService.cs
├── Interfaces/
│   └── IYourService.cs
├── Exceptions/
│   └── NotFoundException.cs
└── Mappings/
    └── MappingProfile.cs
```

### Infrastructure Layer
```
Certus.Infrastructure/
├── Data/
│   └── CertusDbContext.cs
├── Repositories/
│   └── YourRepository.cs
├── Persistence/
│   └── UnitOfWork.cs
└── Configurations/
    └── YourEntityConfiguration.cs
```

## Testing Strategy

- **Unit Tests**: Test individual services/entities in isolation
- **Integration Tests**: Test API endpoints with real database (in-memory or test DB)
- **Controllers**: Test request validation and response formatting
- **Services**: Test business logic with mocked dependencies

## Getting Started

1. Create the projects using the SETUP.md commands
2. Implement Domain entities and interfaces
3. Create Application DTOs and services
4. Implement Infrastructure data access
5. Expose through API controllers
6. Build UI components
7. Write comprehensive tests
