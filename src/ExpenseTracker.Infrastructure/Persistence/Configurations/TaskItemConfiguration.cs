using ExpenseTracker.Domain.Tasks;
using ExpenseTracker.Infrastructure.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Infrastructure.Persistence.Configurations;

internal sealed class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("Tasks");

        builder.HasKey(task => task.Id);

        builder.Property(task => task.Id)
            .ValueGeneratedOnAdd();

        builder.Property(task => task.UserId)
            .IsRequired();

        builder.Property(task => task.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(task => task.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(task => task.Status)
            .HasConversion(
                status => ToStorageValue(status),
                value => FromStorageValue(value))
            .HasColumnType("TEXT")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(task => task.DueDate)
            .HasColumnType("TEXT")
            .IsRequired();

        builder.HasIndex(task => new { task.UserId, task.Id });

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(task => task.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static string ToStorageValue(
        ExpenseTracker.Domain.Tasks.TaskStatus status) => status switch
        {
            ExpenseTracker.Domain.Tasks.TaskStatus.Pending => "pending",
            ExpenseTracker.Domain.Tasks.TaskStatus.InProgress => "inProgress",
            ExpenseTracker.Domain.Tasks.TaskStatus.Completed => "completed",
            _ => throw new InvalidOperationException("Task status is invalid.")
        };

    private static ExpenseTracker.Domain.Tasks.TaskStatus FromStorageValue(
        string status) => status switch
        {
            "pending" => ExpenseTracker.Domain.Tasks.TaskStatus.Pending,
            "inProgress" => ExpenseTracker.Domain.Tasks.TaskStatus.InProgress,
            "completed" => ExpenseTracker.Domain.Tasks.TaskStatus.Completed,
            _ => throw new InvalidOperationException("Task status is invalid.")
        };
}
