using Certus.Domain.Evaluation.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Certus.Infrastructure.Persistence.Configurations;

public class PerformanceSnapshotConfiguration : IEntityTypeConfiguration<PerformanceSnapshot>
{
    public void Configure(EntityTypeBuilder<PerformanceSnapshot> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.ActualReturn).HasPrecision(18, 6);
        builder.Property(p => p.SupposedReturn).HasPrecision(18, 6);
        builder.Property(p => p.DailyPnL).HasPrecision(18, 2);
        builder.Property(p => p.Equity).HasPrecision(18, 2);
        builder.Property(p => p.Drawdown).HasPrecision(18, 6);
        builder.Property(p => p.Sharpe).HasPrecision(18, 6);
    }
}
