using ExpenseTracker.Domain.Tasks;
using ExpenseTracker.Domain.Common;
using Xunit;

namespace ExpenseTracker.Domain.UnitTests.Tasks;

public sealed class TaskItemTests
{
    [Fact]
    public void Create_WithValidValues_PreservesOwnedTaskData()
    {
        var userId = Guid.NewGuid();
        var dueDate = new DateOnly(2026, 8, 15);

        var task = new TaskItem(
            userId,
            "Prepare interview",
            "Review the implementation",
            ExpenseTracker.Domain.Tasks.TaskStatus.Pending,
            dueDate);

        Assert.Equal(0, task.Id);
        Assert.Equal(userId, task.UserId);
        Assert.Equal("Prepare interview", task.Title);
        Assert.Equal("Review the implementation", task.Description);
        Assert.Equal(ExpenseTracker.Domain.Tasks.TaskStatus.Pending, task.Status);
        Assert.Equal(dueDate, task.DueDate);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithMissingTitle_ThrowsDomainValidationException(string? title)
    {
        var createTask = () => new TaskItem(
            Guid.NewGuid(),
            title!,
            null,
            ExpenseTracker.Domain.Tasks.TaskStatus.Pending,
            new DateOnly(2026, 8, 15));

        Assert.Throws<DomainValidationException>(createTask);
    }

    [Fact]
    public void Create_WithTitleLongerThan100Characters_ThrowsDomainValidationException()
    {
        var createTask = () => new TaskItem(
            Guid.NewGuid(),
            new string('a', 101),
            null,
            ExpenseTracker.Domain.Tasks.TaskStatus.Pending,
            new DateOnly(2026, 8, 15));

        Assert.Throws<DomainValidationException>(createTask);
    }

    [Fact]
    public void Create_TrimsTitleAndDescription()
    {
        var task = new TaskItem(
            Guid.NewGuid(),
            "  Prepare interview  ",
            "  Review the implementation  ",
            ExpenseTracker.Domain.Tasks.TaskStatus.Pending,
            new DateOnly(2026, 8, 15));

        Assert.Equal("Prepare interview", task.Title);
        Assert.Equal("Review the implementation", task.Description);
    }

    [Fact]
    public void Create_WithMissingDescription_StoresEmptyDescription()
    {
        var task = new TaskItem(
            Guid.NewGuid(),
            "Prepare interview",
            null,
            ExpenseTracker.Domain.Tasks.TaskStatus.Pending,
            new DateOnly(2026, 8, 15));

        Assert.Equal(string.Empty, task.Description);
    }

    [Fact]
    public void Create_WithDescriptionLongerThan1000Characters_ThrowsDomainValidationException()
    {
        var createTask = () => new TaskItem(
            Guid.NewGuid(),
            "Prepare interview",
            new string('a', 1001),
            ExpenseTracker.Domain.Tasks.TaskStatus.Pending,
            new DateOnly(2026, 8, 15));

        Assert.Throws<DomainValidationException>(createTask);
    }

    [Fact]
    public void Create_WithUndefinedStatus_ThrowsDomainValidationException()
    {
        var createTask = () => new TaskItem(
            Guid.NewGuid(),
            "Prepare interview",
            null,
            (ExpenseTracker.Domain.Tasks.TaskStatus)999,
            new DateOnly(2026, 8, 15));

        Assert.Throws<DomainValidationException>(createTask);
    }

    [Fact]
    public void Update_WithValidValues_UpdatesAllMutableFields()
    {
        var task = CreateTask();
        var dueDate = new DateOnly(2026, 9, 1);

        task.Update(
            "  Complete interview  ",
            "  Send the finished exercise  ",
            ExpenseTracker.Domain.Tasks.TaskStatus.Completed,
            dueDate);

        Assert.Equal("Complete interview", task.Title);
        Assert.Equal("Send the finished exercise", task.Description);
        Assert.Equal(ExpenseTracker.Domain.Tasks.TaskStatus.Completed, task.Status);
        Assert.Equal(dueDate, task.DueDate);
    }

    [Fact]
    public void Update_WithInvalidTitle_ThrowsAndPreservesAllExistingValues()
    {
        var task = CreateTask();

        var updateTask = () => task.Update(
            "   ",
            "Changed description",
            ExpenseTracker.Domain.Tasks.TaskStatus.Completed,
            new DateOnly(2026, 9, 1));

        Assert.Throws<DomainValidationException>(updateTask);
        Assert.Equal("Prepare interview", task.Title);
        Assert.Equal("Review the implementation", task.Description);
        Assert.Equal(ExpenseTracker.Domain.Tasks.TaskStatus.Pending, task.Status);
        Assert.Equal(new DateOnly(2026, 8, 15), task.DueDate);
    }

    [Fact]
    public void Update_WithInvalidDescription_ThrowsAndPreservesAllExistingValues()
    {
        var task = CreateTask();

        var updateTask = () => task.Update(
            "Changed title",
            new string('a', 1001),
            ExpenseTracker.Domain.Tasks.TaskStatus.Completed,
            new DateOnly(2026, 9, 1));

        Assert.Throws<DomainValidationException>(updateTask);
        Assert.Equal("Prepare interview", task.Title);
        Assert.Equal("Review the implementation", task.Description);
        Assert.Equal(ExpenseTracker.Domain.Tasks.TaskStatus.Pending, task.Status);
        Assert.Equal(new DateOnly(2026, 8, 15), task.DueDate);
    }

    private static TaskItem CreateTask() => new(
        Guid.NewGuid(),
        "Prepare interview",
        "Review the implementation",
        ExpenseTracker.Domain.Tasks.TaskStatus.Pending,
        new DateOnly(2026, 8, 15));
}
