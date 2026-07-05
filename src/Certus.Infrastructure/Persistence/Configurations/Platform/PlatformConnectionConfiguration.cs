using Certus.Domain.Platform.Aggregates;
using Certus.Domain.Platform.Enums;
using Certus.Domain.Platform.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Certus.Infrastructure.Persistence.Configurations.Platform;

public class PlatformConnectionConfiguration : IEntityTypeConfiguration<PlatformConnection>
{
    public void Configure(EntityTypeBuilder<PlatformConnection> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.PlatformId).HasMaxLength(100).IsRequired();
        builder.Property(p => p.PlatformName).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(50);
        builder.Property(p => p.ErrorMessage).HasMaxLength(2000);
        
        builder.OwnsOne(p => p.Config, c =>
        {
            c.Property(c => c.PlatformType).HasColumnName("ConfigPlatformType").HasConversion<string>().HasMaxLength(50);
            c.Property(c => c.DataFormat).HasColumnName("ConfigDataFormat").HasConversion<string>().HasMaxLength(50);
            c.Property(c => c.FilePath).HasColumnName("ConfigFilePath").HasMaxLength(500);
            c.Property(c => c.ServerAddress).HasColumnName("ConfigServerAddress").HasMaxLength(200);
            c.Property(c => c.Port).HasColumnName("ConfigPort");
            c.Property(c => c.ApiKey).HasColumnName("ConfigApiKey").HasMaxLength(500);
            c.Property(c => c.PollingIntervalMs).HasColumnName("ConfigPollingIntervalMs");
            c.Property(c => c.UseFileWatcher).HasColumnName("ConfigUseFileWatcher");
        });

        builder.Ignore(p => p.DomainEvents);
    }
}
