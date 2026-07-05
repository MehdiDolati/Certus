using Certus.Domain.Execution.Aggregates;
using Certus.Domain.Execution.Enums;
using Certus.Domain.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Certus.Infrastructure.Persistence.Configurations;

public class TradeConfiguration : IEntityTypeConfiguration<Trade>
{
    public void Configure(EntityTypeBuilder<Trade> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.EntryPrice).HasPrecision(18, 6);
        builder.Property(t => t.ExitPrice).HasPrecision(18, 6);
        builder.Property(t => t.Quantity).HasPrecision(18, 8);
        builder.Property(t => t.PnL).HasPrecision(18, 2);
        builder.Property(t => t.PnLPercent).HasPrecision(18, 6);
        builder.Property(t => t.Fees).HasPrecision(18, 2);
        builder.Property(t => t.Slippage).HasPrecision(18, 2);
        builder.Property(t => t.AgentReason).HasMaxLength(2000);
        builder.Property(t => t.Side).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(50);

        builder.OwnsOne(t => t.Symbol, s =>
        {
            s.Property(s => s.Value).HasColumnName("Symbol").HasMaxLength(20);
            s.Property(s => s.AssetClass).HasColumnName("AssetClass").HasConversion<string>().HasMaxLength(20);
        });

        builder.Ignore(t => t.Orders);
        builder.Ignore(t => t.ExecutionLogs);
        builder.Ignore(t => t.Lifecycle);
        builder.Ignore(t => t.DomainEvents);
        builder.Ignore(t => t.IsOpen);
        builder.Ignore(t => t.IsClosed);
        builder.Ignore(t => t.Duration);
        builder.Ignore(t => t.IsWinning);
    }
}
