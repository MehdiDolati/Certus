using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Certus.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<CertusDbContext>
{
    public CertusDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "Certus.Dashboard"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=localhost;Database=Certus;User=sa;Password=Certus@2026!;TrustServerCertificate=True";

        var optionsBuilder = new DbContextOptionsBuilder<CertusDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new CertusDbContext(optionsBuilder.Options);
    }
}
