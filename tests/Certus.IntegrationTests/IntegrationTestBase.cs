using Certus.Application.Portfolios;
using Certus.Application.Strategies;
using Certus.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Certus.IntegrationTests;

public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>, IDisposable
{
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly IServiceScope Scope;
    protected readonly CertusDbContext DbContext;

    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Scope = Factory.Services.CreateScope();
        DbContext = Scope.ServiceProvider.GetRequiredService<CertusDbContext>();
    }

    protected IPortfolioService GetPortfolioService()
        => Scope.ServiceProvider.GetRequiredService<IPortfolioService>();

    protected IStrategyService GetStrategyService()
        => Scope.ServiceProvider.GetRequiredService<IStrategyService>();

    protected void ClearTracker() => DbContext.ChangeTracker.Clear();

    public void Dispose()
    {
        Scope?.Dispose();
    }
}
