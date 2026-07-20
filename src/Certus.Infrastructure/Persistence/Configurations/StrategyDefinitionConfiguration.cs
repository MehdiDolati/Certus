using Certus.Domain.Strategy.Aggregates;
using Certus.Domain.Strategy.Enums;
using Certus.Domain.Strategy.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Certus.Infrastructure.Persistence.Configurations;

public class StrategyDefinitionConfiguration : IEntityTypeConfiguration<StrategyDefinition>
{
    public void Configure(EntityTypeBuilder<StrategyDefinition> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Description).HasMaxLength(1000);
        builder.Property(s => s.TargetReturn).HasPrecision(18, 6);
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(50);

        builder.OwnsOne(s => s.Type, t =>
        {
            t.Property(t => t.Category).HasColumnName("TypeCategory").HasConversion<string>().HasMaxLength(50);
            t.Property(t => t.SubType).HasColumnName("TypeSubType").HasMaxLength(100);
        });

        builder.Ignore(s => s.Parameters);
        builder.Ignore(s => s.BacktestRuns);
        builder.Ignore(s => s.RiskProfile);
        builder.Ignore(s => s.PredictedPerformance);
        builder.Ignore(s => s.DomainEvents);
        builder.Ignore(s => s.IsActive);
        builder.Ignore(s => s.CanBeActivated);
    }
}
