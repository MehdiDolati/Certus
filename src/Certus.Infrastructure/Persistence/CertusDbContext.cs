using Certus.Domain.RiskAndPortfolio.Aggregates;
using Certus.Domain.Strategy.Aggregates;
using Certus.Domain.Strategy.Entities;
using Certus.Domain.Execution.Aggregates;
using Certus.Domain.Evaluation.Entities;
using Certus.Domain.Evaluation.Aggregates;
using Certus.Domain.Platform.Aggregates;
using Certus.Domain.Platform.Entities;
using Certus.Domain.Platform.Repositories;
using Certus.Domain.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Certus.Infrastructure.Persistence;

public class CertusDbContext : DbContext
{
    public CertusDbContext(DbContextOptions<CertusDbContext> options) : base(options) { }

    // RiskAndPortfolio
    public DbSet<Portfolio> Portfolios => Set<Portfolio>();

    // Strategy
    public DbSet<StrategyDefinition> Strategies => Set<StrategyDefinition>();
    public DbSet<StrategySlot> StrategySlots => Set<StrategySlot>();

    // Execution
    public DbSet<Trade> Trades => Set<Trade>();

    // Evaluation
    public DbSet<PerformanceSnapshot> PerformanceSnapshots => Set<PerformanceSnapshot>();

    // Platform
    public DbSet<PlatformConnection> PlatformConnections => Set<PlatformConnection>();
    public DbSet<PortfolioDeployment> PortfolioDeployments => Set<PortfolioDeployment>();
    public DbSet<ImportedTrade> ImportedTrades => Set<ImportedTrade>();
    public DbSet<StrategyMapping> StrategyMappings => Set<StrategyMapping>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CertusDbContext).Assembly);

        modelBuilder.Ignore<Domain.SharedKernel.IDomainEvent>();

        // Ignore entities not yet needed — will be enabled as contexts are built out
        modelBuilder.Ignore<PerformanceReport>();

        // Configure ImportedTrade as owned entity or separate table
        modelBuilder.Entity<ImportedTrade>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ExternalId).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Symbol).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Side).HasMaxLength(10).IsRequired();
            entity.Property(e => e.Comment).HasMaxLength(500);
            entity.HasIndex(e => e.ExternalId).IsUnique();
            entity.HasIndex(e => e.StrategyId);
            entity.HasIndex(e => e.PortfolioId);
            entity.HasIndex(e => e.OpenTime);
        });
    }
}
