# Implementation Checklist

## ✅ Phase 1: Project Setup

- [ ] Run setup commands from SETUP.md
- [ ] Verify all 7 projects are created
- [ ] Verify project references are correctly added
- [ ] Verify solution builds successfully
- [ ] Verify tests run (should be empty but runnable)

## ✅ Phase 2: Domain Layer (Certus.Domain)

### Entities
- [ ] Create `Common/BaseEntity.cs` - base class with Id, CreatedAt, UpdatedAt
- [ ] Create domain entities in `Entities/` folder
- [ ] Define aggregate roots
- [ ] Implement value objects in `ValueObjects/` if needed

### Interfaces
- [ ] Create `IRepository<T>` interface
- [ ] Create domain service interfaces
- [ ] Add domain events if applicable

### Example Structure
```
Certus.Domain/
├── Common/
│   └── BaseEntity.cs
├── Entities/
│   └── Product.cs
├── ValueObjects/
│   └── Price.cs
└── Interfaces/
    └── IProductRepository.cs
```

## ✅ Phase 3: Application Layer (Certus.Application)

### DTOs
- [ ] Create request DTOs for each entity
- [ ] Create response DTOs for each entity
- [ ] Keep DTOs simple (properties only)

### Services
- [ ] Create application services in `Services/`
- [ ] Define service interfaces in `Interfaces/`
- [ ] Implement CRUD operations

### Exceptions
- [ ] Create `NotFoundException`
- [ ] Create `ValidationException`
- [ ] Create custom domain exceptions

### Mapping
- [ ] Create AutoMapper profile in `Mappings/MappingProfile.cs`
- [ ] Configure Entity → DTO mappings
- [ ] Configure DTO → Entity mappings

### Example Structure
```
Certus.Application/
├── DTOs/
│   ├── ProductCreateDto.cs
│   ├── ProductUpdateDto.cs
│   └── ProductResponseDto.cs
├── Services/
│   └── ProductService.cs
├── Interfaces/
│   └── IProductService.cs
├── Mappings/
│   └── MappingProfile.cs
└── Exceptions/
    ├── NotFoundException.cs
    └── ValidationException.cs
```

## ✅ Phase 4: Infrastructure Layer (Certus.Infrastructure)

### Database Context
- [ ] Create `CertusDbContext.cs` extending DbContext
- [ ] Register DbSets for each entity
- [ ] Configure connection strings

### Repositories
- [ ] Implement `IRepository<T>` for each entity
- [ ] Create entity repositories in `Repositories/`
- [ ] Implement CRUD methods
- [ ] Add query methods as needed

### Entity Configurations
- [ ] Create FluentAPI configurations in `Configurations/`
- [ ] Configure primary keys
- [ ] Configure relationships
- [ ] Configure constraints

### Migrations
- [ ] Add initial migration
- [ ] Create database schema

### Example Structure
```
Certus.Infrastructure/
├── Data/
│   └── CertusDbContext.cs
├── Repositories/
│   └── ProductRepository.cs
├── Configurations/
│   └── ProductConfiguration.cs
└── Persistence/
    └── UnitOfWork.cs (optional)
```

## ✅ Phase 5: API Layer (Certus.API)

### Program.cs Configuration
- [ ] Configure dependency injection
- [ ] Register DbContext
- [ ] Register repositories
- [ ] Register application services
- [ ] Configure AutoMapper
- [ ] Configure Swagger/OpenAPI
- [ ] Add middleware (logging, error handling)

### Controllers
- [ ] Create base controller with common functionality
- [ ] Create REST controllers for each entity
- [ ] Implement GET, POST, PUT, DELETE endpoints
- [ ] Add route attributes
- [ ] Add XML documentation comments

### Models
- [ ] Create request models for POST/PUT
- [ ] Create response models for GET
- [ ] Keep separate from DTOs if needed for API compatibility

### Middleware
- [ ] Create exception handling middleware
- [ ] Create logging middleware (optional)
- [ ] Add CORS configuration if needed

### Example Structure
```
Certus.API/
├── Controllers/
│   ├── BaseController.cs
│   └── ProductsController.cs
├── Models/
│   ├── CreateProductRequest.cs
│   └── ProductResponse.cs
├── Middleware/
│   └── ExceptionHandlingMiddleware.cs
└── Program.cs
```

## ✅ Phase 6: UI Layer (Certus.UI)

### Services
- [ ] Create HTTP client service
- [ ] Create API client wrapper
- [ ] Implement method for each API endpoint

### Components
- [ ] Create layout components
- [ ] Create reusable components
- [ ] Implement data-bound components

### Pages
- [ ] Create list pages (read data from API)
- [ ] Create detail pages (view single item)
- [ ] Create create/edit pages (post to API)

### State Management (Optional)
- [ ] Implement if complex state needed
- [ ] Use Cascading Parameters if simple

### Example Structure
```
Certus.UI/
├── Components/
│   ├── Layout/
│   │   └── MainLayout.razor
│   └── Product/
│       └── ProductCard.razor
├── Pages/
│   ├── Products/
│   │   ├── Index.razor
│   │   ├── Detail.razor
│   │   └── Create.razor
└── Services/
    └── ProductApiClient.cs
```

## ✅ Phase 7: Unit Tests (Certus.Tests.Unit)

### Service Tests
- [ ] Test each application service method
- [ ] Mock repositories
- [ ] Test success scenarios
- [ ] Test error/exception scenarios
- [ ] Test validation

### Example Test Structure
```csharp
public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _mockRepo;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _mockRepo = new Mock<IProductRepository>();
        _service = new ProductService(_mockRepo.Object);
    }

    [Fact]
    public async Task GetProduct_WithValidId_ReturnsProduct()
    {
        // Arrange
        var productId = 1;
        var product = new Product { Id = productId, Name = "Test" };
        _mockRepo.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(product);

        // Act
        var result = await _service.GetProductAsync(productId);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Test");
    }
}
```

## ✅ Phase 8: Integration Tests (Certus.Tests.Integration)

### API Endpoint Tests
- [ ] Test each endpoint with HTTP requests
- [ ] Use WebApplicationFactory for test server
- [ ] Test success scenarios (200, 201)
- [ ] Test error scenarios (400, 404, 500)
- [ ] Test authentication/authorization if applicable

### Database Tests
- [ ] Use test database (in-memory or separate instance)
- [ ] Test repository methods with real DB
- [ ] Clean up data between tests

### Example Test Structure
```csharp
public class ProductsControllerTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _httpClient;

    public ProductsControllerTests()
    {
        _factory = new WebApplicationFactory<Program>();
        _httpClient = _factory.CreateClient();
    }

    [Fact]
    public async Task GetProducts_Returns200()
    {
        // Act
        var response = await _httpClient.GetAsync("/api/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

## ✅ Phase 9: Build Pipeline

- [ ] Verify GitHub Actions workflow triggers
- [ ] Verify all tests pass in CI
- [ ] Verify artifacts are published
- [ ] Set up branch protection rules
- [ ] Configure Azure Pipelines if using

## ✅ Phase 10: Documentation

- [ ] Update README with project description
- [ ] Document API endpoints in Swagger
- [ ] Add XML documentation to public methods
- [ ] Create deployment guide
- [ ] Document environment configuration

## ✅ Phase 11: Deployment

- [ ] Test Docker build
- [ ] Test Docker Compose deployment
- [ ] Configure production appsettings
- [ ] Set up database backup strategy
- [ ] Document deployment process

## Notes

- Follow the dependency flow: UI/API → Application → Domain ← Infrastructure
- Keep domain layer free of external dependencies
- Use dependency injection throughout
- Write tests as you implement features
- Review ARCHITECTURE.md regularly
- Update this checklist as you progress
