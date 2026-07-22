using ExpenseTracker.Application.Abstractions.Persistence;
using ExpenseTracker.Application.Abstractions.Security;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.Tasks.Models;
using ExpenseTracker.Domain.Common;
using ExpenseTracker.Domain.Tasks;

namespace ExpenseTracker.Application.Tasks;

public class TaskService : ITaskService
{
    private readonly ICurrentUser _currentUser;
    private readonly ITaskRepository _taskRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TaskService(
        ICurrentUser currentUser,
        ITaskRepository taskRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _taskRepository = taskRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TaskResult>> CreateAsync(
        CreateTaskCommand command,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return Unauthorized<TaskResult>();
        }

        if (!TryParseStatus(command.Status, out var status))
        {
            return Validation<TaskResult>("Task status is invalid.");
        }

        TaskItem task;
        try
        {
            task = new TaskItem(
                _currentUser.UserId,
                command.Title,
                command.Description,
                status,
                command.DueDate);
        }
        catch (DomainValidationException exception)
        {
            return Validation<TaskResult>(exception.Message);
        }

        await _taskRepository.AddAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<TaskResult>.Success(Map(task));
    }

    public async Task<Result<IReadOnlyList<TaskResult>>> ListAsync(
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return Unauthorized<IReadOnlyList<TaskResult>>();
        }

        var tasks = await _taskRepository.ListForUserAsync(
            _currentUser.UserId,
            cancellationToken);
        var results = tasks
            .OrderBy(task => task.Id)
            .Select(Map)
            .ToArray();

        return Result<IReadOnlyList<TaskResult>>.Success(results);
    }

    public async Task<Result<TaskResult>> GetAsync(
        long taskId,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return Unauthorized<TaskResult>();
        }

        var task = await Find(taskId, cancellationToken);

        return task is null
            ? NotFound<TaskResult>()
            : Result<TaskResult>.Success(Map(task));
    }

    public async Task<Result<TaskResult>> UpdateAsync(
        long taskId,
        UpdateTaskCommand command,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return Unauthorized<TaskResult>();
        }

        var task = await Find(taskId, cancellationToken);
        if (task is null)
        {
            return NotFound<TaskResult>();
        }

        if (!TryParseStatus(command.Status, out var status))
        {
            return Validation<TaskResult>("Task status is invalid.");
        }

        try
        {
            task.Update(
                command.Title,
                command.Description,
                status,
                command.DueDate);
        }
        catch (DomainValidationException exception)
        {
            return Validation<TaskResult>(exception.Message);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<TaskResult>.Success(Map(task));
    }

    public async Task<Result<bool>> DeleteAsync(
        long taskId,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return Unauthorized<bool>();
        }

        var task = await Find(taskId, cancellationToken);
        if (task is null)
        {
            return NotFound<bool>();
        }

        _taskRepository.Remove(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }

    private Task<TaskItem?> Find(
        long taskId,
        CancellationToken cancellationToken) =>
        _taskRepository.GetByIdForUserAsync(
            taskId,
            _currentUser.UserId,
            cancellationToken);

    private static bool TryParseStatus(
        string status,
        out ExpenseTracker.Domain.Tasks.TaskStatus parsedStatus)
    {
        parsedStatus = status switch
        {
            "pending" => ExpenseTracker.Domain.Tasks.TaskStatus.Pending,
            "inProgress" => ExpenseTracker.Domain.Tasks.TaskStatus.InProgress,
            "completed" => ExpenseTracker.Domain.Tasks.TaskStatus.Completed,
            _ => default
        };

        return status is "pending" or "inProgress" or "completed";
    }

    private static TaskResult Map(TaskItem task) => new(
        task.Id,
        task.Title,
        task.Description,
        task.Status switch
        {
            ExpenseTracker.Domain.Tasks.TaskStatus.Pending => "pending",
            ExpenseTracker.Domain.Tasks.TaskStatus.InProgress => "inProgress",
            ExpenseTracker.Domain.Tasks.TaskStatus.Completed => "completed",
            _ => throw new InvalidOperationException("Task status is invalid.")
        },
        task.DueDate);

    private static Result<T> Unauthorized<T>() => Result<T>.Failure(
        new Error("Unauthorized", "Authentication is required.", ErrorType.Unauthorized));

    private static Result<T> NotFound<T>() => Result<T>.Failure(
        new Error("NotFound", "The requested resource was not found.", ErrorType.NotFound));

    private static Result<T> Validation<T>(string message) => Result<T>.Failure(
        new Error("TaskValidation", message, ErrorType.Validation));
}
