using Certus.Domain.Platform.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Certus.Infrastructure.Persistence.Configurations;

public class StrategyMappingConfiguration : IEntityTypeConfiguration<StrategyMapping>
{
    public void Configure(EntityTypeBuilder<StrategyMapping> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.StrategyExternalId).HasMaxLength(100).IsRequired();

        builder.HasIndex(m => new { m.ConnectionId, m.StrategyExternalId }).IsUnique();
        builder.HasIndex(m => m.ConnectionId);
        builder.HasIndex(m => m.StrategyDefinitionId);
    }
}
