# One-Time Setup Commands

Copy and paste these commands into PowerShell or Command Prompt from the `Certus` directory.

## 1. Create Solution and Projects

```powershell
dotnet new sln -n Certus
dotnet new classlib -n Certus.Domain -f net9.0
dotnet new classlib -n Certus.Application -f net9.0
dotnet new classlib -n Certus.Infrastructure -f net9.0
dotnet new webapi -n Certus.API -f net9.0
dotnet new blazor -n Certus.UI -f net9.0 --use-program-main --no-https
dotnet new xunit -n Certus.Tests.Unit -f net9.0
dotnet new xunit -n Certus.Tests.Integration -f net9.0
```

## 2. Add Projects to Solution

```powershell
dotnet sln Certus.sln add Certus.Domain/Certus.Domain.csproj
dotnet sln Certus.sln add Certus.Application/Certus.Application.csproj
dotnet sln Certus.sln add Certus.Infrastructure/Certus.Infrastructure.csproj
dotnet sln Certus.sln add Certus.API/Certus.API.csproj
dotnet sln Certus.sln add Certus.UI/Certus.UI.csproj
dotnet sln Certus.sln add Certus.Tests.Unit/Certus.Tests.Unit.csproj
dotnet sln Certus.sln add Certus.Tests.Integration/Certus.Tests.Integration.csproj
```

## 3. Add Project References

```powershell
# Application depends on Domain
cd Certus.Application
dotnet add reference ../Certus.Domain/Certus.Domain.csproj
cd ..

# Infrastructure depends on Domain and Application
cd Certus.Infrastructure
dotnet add reference ../Certus.Domain/Certus.Domain.csproj
dotnet add reference ../Certus.Application/Certus.Application.csproj
cd ..

# API depends on Application and Infrastructure
cd Certus.API
dotnet add reference ../Certus.Application/Certus.Application.csproj
dotnet add reference ../Certus.Infrastructure/Certus.Infrastructure.csproj
cd ..

# UI depends on Application
cd Certus.UI
dotnet add reference ../Certus.Application/Certus.Application.csproj
cd ..

# Unit Tests depend on Application and Infrastructure
cd Certus.Tests.Unit
dotnet add reference ../Certus.Application/Certus.Application.csproj
dotnet add reference ../Certus.Infrastructure/Certus.Infrastructure.csproj
cd ..

# Integration Tests depend on API and Infrastructure
cd Certus.Tests.Integration
dotnet add reference ../Certus.API/Certus.API.csproj
dotnet add reference ../Certus.Infrastructure/Certus.Infrastructure.csproj
cd ..
```

## 4. Add NuGet Packages to Infrastructure

```powershell
cd Certus.Infrastructure
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
cd ..
```

## 5. Add NuGet Packages to API

```powershell
cd Certus.API
dotnet add package Swashbuckle.AspNetCore
cd ..
```

## 6. Add NuGet Packages to Unit Tests

```powershell
cd Certus.Tests.Unit
dotnet add package Moq
dotnet add package FluentAssertions
cd ..
```

## 7. Add NuGet Packages to Integration Tests

```powershell
cd Certus.Tests.Integration
dotnet add package Microsoft.AspNetCore.Mvc.Testing
dotnet add package Moq
cd ..
```

## 8. Restore and Build

```powershell
cd ..
dotnet restore
dotnet build
dotnet test
```

## Verify Success

After running all commands, you should see:
- ✅ All 7 projects created
- ✅ Solution builds without errors
- ✅ Tests run (even though they're empty)
- ✅ Solution loads in Visual Studio

## Next Steps

1. Open `Certus.sln` in Visual Studio 2022
2. Review `ARCHITECTURE.md` for layer responsibilities
3. Follow `CHECKLIST.md` for implementation steps
4. Start implementing domain entities in `Certus.Domain`
5. Implement application services in `Certus.Application`
6. Create repositories in `Certus.Infrastructure`
7. Build API endpoints in `Certus.API`
8. Create UI components in `Certus.UI`
9. Write tests as you go

## Troubleshooting

**Issue**: "dotnet: command not found"
- **Solution**: Ensure .NET 9 SDK is installed and added to PATH

**Issue**: Project references fail to add
- **Solution**: Run commands from the Certus root directory

**Issue**: Build fails with missing packages
- **Solution**: Run `dotnet restore` before building

**Issue**: Tests don't run
- **Solution**: Verify you have xUnit packages: `dotnet list package`
