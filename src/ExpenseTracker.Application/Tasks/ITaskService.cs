using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.Tasks.Models;

namespace ExpenseTracker.Application.Tasks;

public interface ITaskService
{
    Task<Result<TaskResult>> CreateAsync(
        CreateTaskCommand command,
        CancellationToken cancellationToken);

    Task<Result<bool>> DeleteAsync(
        long taskId,
        CancellationToken cancellationToken);

    Task<Result<TaskResult>> GetAsync(
        long taskId,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<TaskResult>>> ListAsync(
        CancellationToken cancellationToken);

    Task<Result<TaskResult>> UpdateAsync(
        long taskId,
        UpdateTaskCommand command,
        CancellationToken cancellationToken);
}
