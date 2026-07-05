using Certus.Domain.RiskAndPortfolio.Aggregates;
using Certus.Domain.RiskAndPortfolio.Enums;
using Certus.Domain.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Certus.Infrastructure.Persistence.Configurations;

public class PortfolioConfiguration : IEntityTypeConfiguration<Portfolio>
{
    public void Configure(EntityTypeBuilder<Portfolio> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(1000);
        builder.Property(p => p.TargetReturn).HasPrecision(18, 6);
        builder.Property(p => p.TargetSharpe).HasPrecision(18, 6);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(50);
        builder.Property(p => p.CurrentMarketRegime).HasConversion<string>().HasMaxLength(50);

        builder.OwnsOne(p => p.AllocatedCapital, m =>
        {
            m.Property(m => m.Amount).HasColumnName("AllocatedCapital").HasPrecision(18, 2);
            m.Property(m => m.Currency).HasColumnName("AllocatedCurrency").HasConversion<string>().HasMaxLength(10);
        });

        builder.Ignore(p => p.RiskLimits);
        builder.Ignore(p => p.AllocationRules);
        builder.Ignore(p => p.Allocations);
        builder.Ignore(p => p.AdaptiveRiskParams);
        builder.Ignore(p => p.DomainEvents);
        builder.Ignore(p => p.IsActive);
        builder.Ignore(p => p.AllocatedCapitalInMillions);
    }
}
