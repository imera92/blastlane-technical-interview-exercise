using ExpenseTracker.Domain.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Infrastructure.Persistence.Configurations;

internal sealed class BudgetTransactionConfiguration
    : IEntityTypeConfiguration<BudgetTransaction>
{
    public void Configure(EntityTypeBuilder<BudgetTransaction> builder)
    {
        builder.ToTable("BudgetTransactions");

        builder.HasKey(transaction => transaction.Id);

        builder.Property(transaction => transaction.Id)
            .ValueGeneratedOnAdd();

        builder.Property(transaction => transaction.BudgetId)
            .HasField("_budgetId")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .IsRequired();

        builder.Property(transaction => transaction.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(transaction => transaction.Amount)
            .HasConversion(
                amount => MoneyToCentsConverter.ToCents(amount),
                cents => MoneyToCentsConverter.FromCents(cents))
            .HasColumnType("INTEGER")
            .IsRequired();

        builder.Property(transaction => transaction.Date)
            .HasColumnType("TEXT")
            .IsRequired();

        builder.Property(transaction => transaction.CreatedAtUtc)
            .HasConversion(
                timestamp => UtcDateTimeOffsetConverter.ToUtcTicks(timestamp),
                ticks => UtcDateTimeOffsetConverter.FromUtcTicks(ticks))
            .HasColumnType("INTEGER")
            .IsRequired();

        builder.HasIndex(transaction => new
        {
            transaction.BudgetId,
            transaction.Date,
            transaction.CreatedAtUtc,
            transaction.Id
        });
    }
}
