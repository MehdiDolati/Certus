using Certus.Domain.Strategy.Entities;
using Certus.Domain.Strategy.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Certus.Infrastructure.Persistence.Configurations;

public class StrategySlotConfiguration : IEntityTypeConfiguration<StrategySlot>
{
    public void Configure(EntityTypeBuilder<StrategySlot> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.StrategyId).IsRequired();
        builder.Property(s => s.PortfolioId).IsRequired();

        builder.OwnsOne(s => s.Weight, w =>
        {
            w.Property(w => w.Value).HasColumnName("WeightValue").HasPrecision(18, 6);
        });

        builder.Property(s => s.AssignedAt).IsRequired();

        builder.HasIndex(s => s.StrategyId);
        builder.HasIndex(s => s.PortfolioId);
        builder.HasIndex(s => new { s.StrategyId, s.PortfolioId }).IsUnique();
    }
}
