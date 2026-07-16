using Certus.Domain.Platform.Aggregates;
using Certus.Domain.Platform.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Certus.Infrastructure.Persistence.Configurations;

public class PortfolioDeploymentConfiguration : IEntityTypeConfiguration<PortfolioDeployment>
{
    public void Configure(EntityTypeBuilder<PortfolioDeployment> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.SourceFolderPath).HasMaxLength(500).IsRequired();
        builder.Property(d => d.TargetMT4Path).HasMaxLength(500).IsRequired();
        builder.Property(d => d.Status).HasConversion<string>().HasMaxLength(50);
        builder.Property(d => d.ErrorMessage).HasMaxLength(2000);

        builder.Ignore(d => d.IsMonitoring);
        builder.Ignore(d => d.DomainEvents);

        builder.HasIndex(d => d.PortfolioId);
        builder.HasIndex(d => d.ConnectionId);
    }
}
