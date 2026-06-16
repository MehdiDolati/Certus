# Certus Solution Setup Guide

## Prerequisites
- .NET 9 SDK installed
- Visual Studio 2022 or VS Code with C# Dev Kit

## Quick Setup (Run in PowerShell or Command Prompt)

```powershell
# Create solution structure
dotnet new sln -n Certus
dotnet new classlib -n Certus.Domain -f net9.0
dotnet new classlib -n Certus.Application -f net9.0
dotnet new classlib -n Certus.Infrastructure -f net9.0
dotnet new webapi -n Certus.API -f net9.0
dotnet new blazor -n Certus.UI -f net9.0 --use-program-main --no-https
dotnet new xunit -n Certus.Tests.Unit -f net9.0
dotnet new xunit -n Certus.Tests.Integration -f net9.0

# Add projects to solution
dotnet sln Certus.sln add Certus.Domain/Certus.Domain.csproj
dotnet sln Certus.sln add Certus.Application/Certus.Application.csproj
dotnet sln Certus.sln add Certus.Infrastructure/Certus.Infrastructure.csproj
dotnet sln Certus.sln add Certus.API/Certus.API.csproj
dotnet sln Certus.sln add Certus.UI/Certus.UI.csproj
dotnet sln Certus.sln add Certus.Tests.Unit/Certus.Tests.Unit.csproj
dotnet sln Certus.sln add Certus.Tests.Integration/Certus.Tests.Integration.csproj

# Add project references
cd Certus.Application
dotnet add reference ../Certus.Domain/Certus.Domain.csproj
cd ../Certus.Infrastructure
dotnet add reference ../Certus.Domain/Certus.Domain.csproj
dotnet add reference ../Certus.Application/Certus.Application.csproj
cd ../Certus.API
dotnet add reference ../Certus.Application/Certus.Application.csproj
dotnet add reference ../Certus.Infrastructure/Certus.Infrastructure.csproj
cd ../Certus.UI
dotnet add reference ../Certus.Application/Certus.Application.csproj
cd ../Certus.Tests.Unit
dotnet add reference ../Certus.Application/Certus.Application.csproj
dotnet add reference ../Certus.Infrastructure/Certus.Infrastructure.csproj
cd ../Certus.Tests.Integration
dotnet add reference ../Certus.API/Certus.API.csproj
dotnet add reference ../Certus.Infrastructure/Certus.Infrastructure.csproj
cd ..

# Add required NuGet packages
cd Certus.Infrastructure
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
cd ../Certus.API
dotnet add package Swashbuckle.AspNetCore
cd ../Certus.Tests.Unit
dotnet add package Moq
dotnet add package FluentAssertions
cd ../Certus.Tests.Integration
dotnet add package Microsoft.AspNetCore.Mvc.Testing
dotnet add package Moq
cd ..

# Build solution
dotnet build

# Run tests
dotnet test
```

## Solution Architecture

```
Certus/
├── Certus.Domain/              # Core business logic & entities
│   ├── Entities/
│   ├── ValueObjects/
│   └── Interfaces/
├── Certus.Application/         # Use cases & business rules
│   ├── DTOs/
│   ├── Services/
│   ├── Interfaces/
│   └── Exceptions/
├── Certus.Infrastructure/      # Data access & external services
│   ├── Data/
│   ├── Repositories/
│   └── Persistence/
├── Certus.API/                 # REST API layer
│   ├── Controllers/
│   ├── Middleware/
│   └── Program.cs
├── Certus.UI/                  # Blazor Web UI
│   ├── Components/
│   ├── Pages/
│   └── Services/
├── Certus.Tests.Unit/          # Unit tests
└── Certus.Tests.Integration/   # Integration tests
```

## Next Steps
1. Run the PowerShell commands above in the Certus directory
2. Open `Certus.sln` in Visual Studio 2022 or VS Code
3. Configure appsettings.json for your database
4. Create domain entities and implement repositories
5. Set up dependency injection in Certus.API/Program.cs
