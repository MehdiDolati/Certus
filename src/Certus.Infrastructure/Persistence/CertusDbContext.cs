using Certus.Domain.RiskAndPortfolio.Aggregates;
using Certus.Domain.Strategy.Aggregates;
using Certus.Domain.Execution.Aggregates;
using Certus.Domain.Evaluation.Entities;
using Certus.Domain.Evaluation.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace Certus.Infrastructure.Persistence;

public class CertusDbContext : DbContext
{
    public CertusDbContext(DbContextOptions<CertusDbContext> options) : base(options) { }

    // RiskAndPortfolio
    public DbSet<Portfolio> Portfolios => Set<Portfolio>();

    // Strategy
    public DbSet<StrategyDefinition> Strategies => Set<StrategyDefinition>();

    // Execution
    public DbSet<Trade> Trades => Set<Trade>();

    // Evaluation
    public DbSet<PerformanceSnapshot> PerformanceSnapshots => Set<PerformanceSnapshot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CertusDbContext).Assembly);

        modelBuilder.Ignore<Domain.SharedKernel.IDomainEvent>();

        // Ignore entities not yet needed — will be enabled as contexts are built out
        modelBuilder.Ignore<PerformanceReport>();
    }
}
