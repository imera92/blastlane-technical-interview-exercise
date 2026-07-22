using ExpenseTracker.Domain.Tasks;
using ExpenseTracker.Infrastructure.Authentication;
using ExpenseTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace ExpenseTracker.Infrastructure.UnitTests.Persistence;

public sealed class TaskPersistenceModelTests
{
    [Fact]
    public void TaskItem_IsIncludedInPersistenceModel()
    {
        using var context = CreateContext();

        var taskType = context.Model.FindEntityType(typeof(TaskItem));

        Assert.NotNull(taskType);
        Assert.Equal("Tasks", taskType.GetTableName());
    }

    [Fact]
    public void TaskScalars_UseApprovedPersistencePolicies()
    {
        using var context = CreateContext();

        var taskType = context.Model.FindEntityType(typeof(TaskItem))!;
        var id = taskType.FindProperty(nameof(TaskItem.Id));
        var userId = taskType.FindProperty(nameof(TaskItem.UserId));
        var title = taskType.FindProperty(nameof(TaskItem.Title));
        var description = taskType.FindProperty(nameof(TaskItem.Description));
        var dueDate = taskType.FindProperty(nameof(TaskItem.DueDate));

        Assert.Equal(ValueGenerated.OnAdd, id!.ValueGenerated);
        Assert.False(userId!.IsNullable);
        Assert.False(title!.IsNullable);
        Assert.Equal(100, title.GetMaxLength());
        Assert.False(description!.IsNullable);
        Assert.Equal(1000, description.GetMaxLength());
        Assert.False(dueDate!.IsNullable);
        Assert.Equal("TEXT", dueDate.GetColumnType());
    }

    [Fact]
    public void TaskStatus_UsesApprovedTextValues()
    {
        using var context = CreateContext();

        var taskType = context.Model.FindEntityType(typeof(TaskItem))!;
        var status = taskType.FindProperty(nameof(TaskItem.Status));
        var converter = status!.GetTypeMapping().Converter;

        Assert.NotNull(converter);
        Assert.Equal(typeof(ExpenseTracker.Domain.Tasks.TaskStatus), converter.ModelClrType);
        Assert.Equal(typeof(string), converter.ProviderClrType);
        Assert.Equal("TEXT", status.GetColumnType());
        Assert.Equal("pending", converter.ConvertToProvider(ExpenseTracker.Domain.Tasks.TaskStatus.Pending));
        Assert.Equal("inProgress", converter.ConvertToProvider(ExpenseTracker.Domain.Tasks.TaskStatus.InProgress));
        Assert.Equal("completed", converter.ConvertToProvider(ExpenseTracker.Domain.Tasks.TaskStatus.Completed));
    }

    [Fact]
    public void TaskOwnershipAndIndex_UseApprovedPolicies()
    {
        using var context = CreateContext();

        var taskType = context.Model.FindEntityType(typeof(TaskItem))!;
        var ownership = Assert.Single(
            taskType.GetForeignKeys(),
            foreignKey => foreignKey.PrincipalEntityType.ClrType == typeof(ApplicationUser));

        Assert.True(ownership.IsRequired);
        Assert.Equal(DeleteBehavior.Restrict, ownership.DeleteBehavior);
        Assert.Equal(nameof(TaskItem.UserId), Assert.Single(ownership.Properties).Name);
        Assert.Contains(
            taskType.GetIndexes(),
            index => index.Properties
                .Select(property => property.Name)
                .SequenceEqual([nameof(TaskItem.UserId), nameof(TaskItem.Id)]));
    }

    private static ExpenseTrackerDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ExpenseTrackerDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        return new ExpenseTrackerDbContext(options);
    }
}
