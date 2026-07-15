using Certus.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Certus.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private SqliteConnection? _connection;

    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove ALL EF Core database-related services
            var toRemove = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<CertusDbContext>) ||
                d.ServiceType == typeof(CertusDbContext) ||
                d.ImplementationType == typeof(CertusDbContext) ||
                d.ServiceType.FullName?.Contains("EntityFrameworkCore") == true ||
                d.ServiceType.FullName?.Contains("SqlServer") == true).ToList();

            foreach (var descriptor in toRemove)
                services.Remove(descriptor);

            // Add SQLite for testing (EnsureCreated, not migrations)
            services.AddDbContext<CertusDbContext>(options =>
            {
                options.UseSqlite(_connection!);
            });
        });

        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CertusDbContext>();
        db.Database.EnsureCreated();

        return host;
    }

    public new async Task DisposeAsync()
    {
        await (_connection?.DisposeAsync() ?? ValueTask.CompletedTask);
    }
}
