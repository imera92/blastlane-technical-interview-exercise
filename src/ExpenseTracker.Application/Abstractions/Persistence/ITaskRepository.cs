using ExpenseTracker.Domain.Tasks;

namespace ExpenseTracker.Application.Abstractions.Persistence;

public interface ITaskRepository
{
    Task<TaskItem?> GetByIdForUserAsync(
        long taskId,
        Guid userId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<TaskItem>> ListForUserAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task AddAsync(TaskItem task, CancellationToken cancellationToken);

    void Remove(TaskItem task);
}
