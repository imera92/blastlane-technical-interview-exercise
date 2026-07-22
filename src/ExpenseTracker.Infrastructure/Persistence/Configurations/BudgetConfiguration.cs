using ExpenseTracker.Domain.Budgets;
using ExpenseTracker.Infrastructure.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Infrastructure.Persistence.Configurations;

internal sealed class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.ToTable("Budgets");

        builder.HasKey(budget => budget.Id);

        builder.Property(budget => budget.Id)
            .ValueGeneratedOnAdd();

        builder.Property(budget => budget.UserId)
            .IsRequired();

        builder.Property(budget => budget.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(budget => budget.StartingBalance)
            .HasConversion(
                amount => MoneyToCentsConverter.ToCents(amount),
                cents => MoneyToCentsConverter.FromCents(cents))
            .HasColumnType("INTEGER")
            .IsRequired();

        builder.Property(budget => budget.CreatedAtUtc)
            .HasConversion(
                timestamp => UtcDateTimeOffsetConverter.ToUtcTicks(timestamp),
                ticks => UtcDateTimeOffsetConverter.FromUtcTicks(ticks))
            .HasColumnType("INTEGER")
            .IsRequired();

        builder.Ignore(budget => budget.CurrentBalance);

        builder.HasIndex(budget => new
        {
            budget.UserId,
            budget.CreatedAtUtc,
            budget.Id
        });

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(budget => budget.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(budget => budget.Transactions)
            .WithOne()
            .HasForeignKey(transaction => transaction.BudgetId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(budget => budget.Transactions)
            .HasField("_transactions")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
