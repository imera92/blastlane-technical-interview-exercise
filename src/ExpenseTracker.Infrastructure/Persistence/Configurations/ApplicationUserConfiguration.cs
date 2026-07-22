using ExpenseTracker.Infrastructure.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Infrastructure.Persistence.Configurations;

internal sealed class ApplicationUserConfiguration
    : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(user => user.DisplayName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(user => user.CreatedAtUtc)
            .HasConversion(
                timestamp => UtcDateTimeOffsetConverter.ToUtcTicks(timestamp),
                ticks => UtcDateTimeOffsetConverter.FromUtcTicks(ticks))
            .HasColumnType("INTEGER")
            .IsRequired();

        builder.HasIndex(user => user.NormalizedEmail)
            .HasDatabaseName("EmailIndex")
            .IsUnique();
    }
}
