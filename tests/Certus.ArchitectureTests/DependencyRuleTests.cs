using System.Reflection;
using Certus.Domain.RiskAndPortfolio.Aggregates;
using FluentAssertions;
using NetArchTest.Rules;

namespace Certus.ArchitectureTests;

public class DependencyRuleTests
{
    private static readonly Assembly DomainAssembly = typeof(Portfolio).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(Certus.Application.Portfolios.IPortfolioService).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(Certus.Infrastructure.Persistence.CertusDbContext).Assembly;

    [Fact]
    public void Domain_Should_Not_Reference_Application()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn("Certus.Application")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Domain should not reference Application. Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Domain_Should_Not_Reference_Infrastructure()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn("Certus.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Domain should not reference Infrastructure. Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Application_Should_Not_Reference_Infrastructure()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn("Certus.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Application should not reference Infrastructure. Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Domain_Should_Not_Depend_On_EntityFramework()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue("Domain should not depend on Entity Framework");
    }

    [Fact]
    public void Application_Should_Not_Depend_On_EntityFramework()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue("Application should not depend on Entity Framework");
    }

    [Fact]
    public void Infrastructure_Should_Implement_Domain_Interfaces()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .That()
            .DoNotHaveNameMatching("<>*") // Exclude compiler-generated anonymous types
            .And().DoNotHaveNameEndingWith("Data") // Exclude internal serialization DTOs
            .And().DoNotHaveNameEndingWith("Info") // Exclude internal serialization DTOs
            .Should()
            .HaveDependencyOn("Certus.Domain")
           .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Infrastructure should reference Domain. Types without dependency: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }
}
