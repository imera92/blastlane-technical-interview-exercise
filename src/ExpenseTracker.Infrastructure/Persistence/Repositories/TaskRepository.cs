using ExpenseTracker.Application.Abstractions.Persistence;
using ExpenseTracker.Domain.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Persistence.Repositories;

internal sealed class TaskRepository : ITaskRepository
{
    private readonly ExpenseTrackerDbContext _dbContext;

    public TaskRepository(ExpenseTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<TaskItem?> GetByIdForUserAsync(
        long taskId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        return _dbContext.Set<TaskItem>().SingleOrDefaultAsync(
            task => task.Id == taskId && task.UserId == userId,
            cancellationToken);
    }

    public async Task<IReadOnlyList<TaskItem>> ListForUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Set<TaskItem>()
            .AsNoTracking()
            .Where(task => task.UserId == userId)
            .OrderBy(task => task.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        TaskItem task,
        CancellationToken cancellationToken)
    {
        await _dbContext.Set<TaskItem>().AddAsync(task, cancellationToken);
    }

    public void Remove(TaskItem task)
    {
        _dbContext.Set<TaskItem>().Remove(task);
    }
}
