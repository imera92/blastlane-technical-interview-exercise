using ExpenseTracker.Application.Abstractions.Persistence;
using ExpenseTracker.Application.Abstractions.Security;
using ExpenseTracker.Application.Tasks;
using ExpenseTracker.Application.Tasks.Models;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Domain.Tasks;
using Xunit;

namespace ExpenseTracker.Application.UnitTests.Tasks;

public sealed class TaskServiceTests
{
    [Fact]
    public async Task CreateAsync_WithAuthenticatedUser_CreatesOwnedTaskAndSavesOnce()
    {
        var userId = Guid.NewGuid();
        var repository = new RecordingTaskRepository();
        var unitOfWork = new RecordingUnitOfWork();
        var service = new TaskService(
            new StubCurrentUser(true, userId),
            repository,
            unitOfWork);
        var command = new CreateTaskCommand(
            "  Prepare interview  ",
            "  Review implementation  ",
            "pending",
            new DateOnly(2026, 8, 15));
        var cancellationToken = new CancellationTokenSource().Token;

        var result = await service.CreateAsync(command, cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal("Prepare interview", result.Value!.Title);
        Assert.Equal("Review implementation", result.Value.Description);
        Assert.Equal("pending", result.Value.Status);
        Assert.Equal(command.DueDate, result.Value.DueDate);
        Assert.Equal(userId, repository.AddedTask!.UserId);
        Assert.Equal(1, repository.AddCallCount);
        Assert.Equal(cancellationToken, repository.ReceivedCancellationToken);
        Assert.Equal(1, unitOfWork.SaveCallCount);
        Assert.Equal(cancellationToken, unitOfWork.ReceivedCancellationToken);
    }

    [Fact]
    public async Task CreateAsync_WithoutAuthenticatedUser_ReturnsUnauthorizedWithoutPersistence()
    {
        var repository = new RecordingTaskRepository();
        var unitOfWork = new RecordingUnitOfWork();
        var service = new TaskService(
            new StubCurrentUser(false, Guid.Empty),
            repository,
            unitOfWork);

        var result = await service.CreateAsync(
            new CreateTaskCommand("Task", null, "pending", new DateOnly(2026, 8, 15)),
            CancellationToken.None);

        AssertFailure(result, ErrorType.Unauthorized);
        Assert.Equal(0, repository.AddCallCount);
        Assert.Equal(0, unitOfWork.SaveCallCount);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("Pending")]
    [InlineData("")]
    public async Task CreateAsync_WithInvalidStatus_ReturnsValidationWithoutPersistence(string status)
    {
        var repository = new RecordingTaskRepository();
        var unitOfWork = new RecordingUnitOfWork();
        var service = CreateService(repository, unitOfWork);

        var result = await service.CreateAsync(
            new CreateTaskCommand("Task", null, status, new DateOnly(2026, 8, 15)),
            CancellationToken.None);

        AssertFailure(result, ErrorType.Validation);
        Assert.Equal(0, repository.AddCallCount);
        Assert.Equal(0, unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidTitle_ReturnsValidationWithoutPersistence()
    {
        var repository = new RecordingTaskRepository();
        var unitOfWork = new RecordingUnitOfWork();
        var service = CreateService(repository, unitOfWork);

        var result = await service.CreateAsync(
            new CreateTaskCommand("   ", null, "pending", new DateOnly(2026, 8, 15)),
            CancellationToken.None);

        AssertFailure(result, ErrorType.Validation);
        Assert.Equal(0, repository.AddCallCount);
        Assert.Equal(0, unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task ListAsync_WithAuthenticatedUser_ListsOnlyCurrentUserInIdOrder()
    {
        var userId = Guid.NewGuid();
        var repository = new RecordingTaskRepository
        {
            TasksToList =
            [
                CreateTask(userId, 20, "Later", ExpenseTracker.Domain.Tasks.TaskStatus.Completed),
                CreateTask(userId, 10, "Earlier", ExpenseTracker.Domain.Tasks.TaskStatus.InProgress)
            ]
        };
        var service = new TaskService(
            new StubCurrentUser(true, userId),
            repository,
            new RecordingUnitOfWork());
        var cancellationToken = new CancellationTokenSource().Token;

        var result = await service.ListAsync(cancellationToken);

        Assert.True(result.IsSuccess);
        var tasks = result.Value!;
        Assert.Equal([10L, 20L], tasks.Select(task => task.Id));
        Assert.Equal(["inProgress", "completed"], tasks.Select(task => task.Status));
        Assert.Equal(userId, repository.ReceivedUserId);
        Assert.Equal(cancellationToken, repository.ReceivedCancellationToken);
    }

    [Fact]
    public async Task GetAsync_WithOwnedTask_ReturnsMappedTask()
    {
        var userId = Guid.NewGuid();
        var repository = new RecordingTaskRepository
        {
            TaskToReturn = CreateTask(userId, 42, "Task", ExpenseTracker.Domain.Tasks.TaskStatus.Pending)
        };
        var service = new TaskService(
            new StubCurrentUser(true, userId),
            repository,
            new RecordingUnitOfWork());

        var result = await service.GetAsync(42, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value!.Id);
        Assert.Equal("pending", result.Value.Status);
        Assert.Equal(42, repository.ReceivedTaskId);
        Assert.Equal(userId, repository.ReceivedUserId);
    }

    [Fact]
    public async Task GetAsync_WithMissingOrForeignTask_ReturnsNotFound()
    {
        var repository = new RecordingTaskRepository();
        var service = CreateService(repository, new RecordingUnitOfWork());

        var result = await service.GetAsync(42, CancellationToken.None);

        AssertFailure(result, ErrorType.NotFound);
    }

    [Fact]
    public async Task UpdateAsync_WithOwnedTask_UpdatesAndSavesOnce()
    {
        var userId = Guid.NewGuid();
        var task = CreateTask(userId, 42, "Old title", ExpenseTracker.Domain.Tasks.TaskStatus.Pending);
        var repository = new RecordingTaskRepository { TaskToReturn = task };
        var unitOfWork = new RecordingUnitOfWork();
        var service = new TaskService(
            new StubCurrentUser(true, userId),
            repository,
            unitOfWork);
        var command = new UpdateTaskCommand(
            "  New title  ",
            "  New description  ",
            "completed",
            new DateOnly(2026, 9, 1));

        var result = await service.UpdateAsync(42, command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("New title", task.Title);
        Assert.Equal("New description", task.Description);
        Assert.Equal(ExpenseTracker.Domain.Tasks.TaskStatus.Completed, task.Status);
        Assert.Equal(command.DueDate, task.DueDate);
        Assert.Equal(1, unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidStatus_PreservesTaskWithoutSaving()
    {
        var userId = Guid.NewGuid();
        var task = CreateTask(userId, 42, "Old title", ExpenseTracker.Domain.Tasks.TaskStatus.Pending);
        var repository = new RecordingTaskRepository { TaskToReturn = task };
        var unitOfWork = new RecordingUnitOfWork();
        var service = new TaskService(
            new StubCurrentUser(true, userId),
            repository,
            unitOfWork);

        var result = await service.UpdateAsync(
            42,
            new UpdateTaskCommand("New title", null, "invalid", new DateOnly(2026, 9, 1)),
            CancellationToken.None);

        AssertFailure(result, ErrorType.Validation);
        Assert.Equal("Old title", task.Title);
        Assert.Equal(ExpenseTracker.Domain.Tasks.TaskStatus.Pending, task.Status);
        Assert.Equal(0, unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidTitle_ReturnsValidationAndDoesNotSave()
    {
        var userId = Guid.NewGuid();
        var task = CreateTask(userId, 42, "Old title", ExpenseTracker.Domain.Tasks.TaskStatus.Pending);
        var repository = new RecordingTaskRepository { TaskToReturn = task };
        var unitOfWork = new RecordingUnitOfWork();
        var service = new TaskService(
            new StubCurrentUser(true, userId),
            repository,
            unitOfWork);

        var result = await service.UpdateAsync(
            42,
            new UpdateTaskCommand("   ", null, "completed", new DateOnly(2026, 9, 1)),
            CancellationToken.None);

        AssertFailure(result, ErrorType.Validation);
        Assert.Equal("Old title", task.Title);
        Assert.Equal(ExpenseTracker.Domain.Tasks.TaskStatus.Pending, task.Status);
        Assert.Equal(0, unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task UpdateAndDelete_WithMissingOrForeignTask_ReturnNotFoundWithoutSaving()
    {
        var repository = new RecordingTaskRepository();
        var unitOfWork = new RecordingUnitOfWork();
        var service = CreateService(repository, unitOfWork);

        var update = await service.UpdateAsync(
            42,
            new UpdateTaskCommand("Task", null, "pending", new DateOnly(2026, 8, 15)),
            CancellationToken.None);
        var delete = await service.DeleteAsync(42, CancellationToken.None);

        AssertFailure(update, ErrorType.NotFound);
        AssertFailure(delete, ErrorType.NotFound);
        Assert.Equal(0, repository.RemoveCallCount);
        Assert.Equal(0, unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task DeleteAsync_WithOwnedTask_RemovesAndSavesOnce()
    {
        var userId = Guid.NewGuid();
        var task = CreateTask(userId, 42, "Task", ExpenseTracker.Domain.Tasks.TaskStatus.Pending);
        var repository = new RecordingTaskRepository { TaskToReturn = task };
        var unitOfWork = new RecordingUnitOfWork();
        var service = new TaskService(
            new StubCurrentUser(true, userId),
            repository,
            unitOfWork);

        var result = await service.DeleteAsync(42, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Same(task, repository.RemovedTask);
        Assert.Equal(1, repository.RemoveCallCount);
        Assert.Equal(1, unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task ReadUpdateAndDelete_WithoutAuthenticatedUser_ReturnUnauthorizedWithoutRepositoryAccess()
    {
        var repository = new RecordingTaskRepository();
        var unitOfWork = new RecordingUnitOfWork();
        var service = new TaskService(
            new StubCurrentUser(false, Guid.Empty),
            repository,
            unitOfWork);

        var list = await service.ListAsync(CancellationToken.None);
        var get = await service.GetAsync(42, CancellationToken.None);
        var update = await service.UpdateAsync(
            42,
            new UpdateTaskCommand("Task", null, "pending", new DateOnly(2026, 8, 15)),
            CancellationToken.None);
        var delete = await service.DeleteAsync(42, CancellationToken.None);

        AssertFailure(list, ErrorType.Unauthorized);
        AssertFailure(get, ErrorType.Unauthorized);
        AssertFailure(update, ErrorType.Unauthorized);
        AssertFailure(delete, ErrorType.Unauthorized);
        Assert.Equal(0, repository.QueryCallCount);
        Assert.Equal(0, repository.RemoveCallCount);
        Assert.Equal(0, unitOfWork.SaveCallCount);
    }

    private static TaskService CreateService(
        RecordingTaskRepository repository,
        RecordingUnitOfWork unitOfWork) => new(
            new StubCurrentUser(true, Guid.NewGuid()),
            repository,
            unitOfWork);

    private static TaskItem CreateTask(
        Guid userId,
        long id,
        string title,
        ExpenseTracker.Domain.Tasks.TaskStatus status)
    {
        var task = new TaskItem(
            userId,
            title,
            "Description",
            status,
            new DateOnly(2026, 8, 15));
        typeof(TaskItem).GetProperty(nameof(TaskItem.Id))!.SetValue(task, id);
        return task;
    }

    private static void AssertFailure<T>(
        Result<T> result,
        ErrorType expectedType)
    {
        Assert.True(result.IsFailure);
        Assert.Equal(expectedType, Assert.Single(result.Errors).Type);
    }

    private sealed record StubCurrentUser(bool IsAuthenticated, Guid UserId) : ICurrentUser;

    private sealed class RecordingTaskRepository : ITaskRepository
    {
        public IReadOnlyList<TaskItem> TasksToList { get; init; } = [];
        public TaskItem? TaskToReturn { get; init; }
        public TaskItem? AddedTask { get; private set; }
        public TaskItem? RemovedTask { get; private set; }
        public int AddCallCount { get; private set; }
        public int RemoveCallCount { get; private set; }
        public int QueryCallCount { get; private set; }
        public long ReceivedTaskId { get; private set; }
        public Guid ReceivedUserId { get; private set; }
        public CancellationToken ReceivedCancellationToken { get; private set; }

        public Task AddAsync(TaskItem task, CancellationToken cancellationToken)
        {
            AddedTask = task;
            AddCallCount++;
            ReceivedCancellationToken = cancellationToken;
            return Task.CompletedTask;
        }

        public Task<TaskItem?> GetByIdForUserAsync(long taskId, Guid userId, CancellationToken cancellationToken)
        {
            QueryCallCount++;
            ReceivedTaskId = taskId;
            ReceivedUserId = userId;
            ReceivedCancellationToken = cancellationToken;
            return Task.FromResult(TaskToReturn);
        }

        public Task<IReadOnlyList<TaskItem>> ListForUserAsync(Guid userId, CancellationToken cancellationToken)
        {
            QueryCallCount++;
            ReceivedUserId = userId;
            ReceivedCancellationToken = cancellationToken;
            return Task.FromResult(TasksToList);
        }

        public void Remove(TaskItem task)
        {
            RemoveCallCount++;
            RemovedTask = task;
        }
    }

    private sealed class RecordingUnitOfWork : IUnitOfWork
    {
        public int SaveCallCount { get; private set; }
        public CancellationToken ReceivedCancellationToken { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveCallCount++;
            ReceivedCancellationToken = cancellationToken;
            return Task.FromResult(1);
        }
    }
}
