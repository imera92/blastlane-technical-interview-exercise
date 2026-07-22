using ExpenseTracker.Application.Abstractions.Persistence;
using ExpenseTracker.Domain.Budgets;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Persistence.Repositories;

internal sealed class BudgetRepository : IBudgetRepository
{
    private readonly ExpenseTrackerDbContext _dbContext;

    public BudgetRepository(ExpenseTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsForUserAsync(
        long budgetId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        return _dbContext.Set<Budget>().AnyAsync(
            budget => budget.Id == budgetId && budget.UserId == userId,
            cancellationToken);
    }

    public Task<Budget?> GetByIdForUserAsync(
        long budgetId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        return _dbContext.Set<Budget>()
            .Include(budget => budget.Transactions)
            .SingleOrDefaultAsync(
                budget => budget.Id == budgetId && budget.UserId == userId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Budget>> ListForUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Set<Budget>()
            .AsNoTracking()
            .Where(budget => budget.UserId == userId)
            .Include(budget => budget.Transactions)
            .OrderBy(budget => budget.CreatedAtUtc)
            .ThenBy(budget => budget.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        Budget budget,
        CancellationToken cancellationToken)
    {
        await _dbContext.Set<Budget>().AddAsync(budget, cancellationToken);
    }

    public void Remove(Budget budget)
    {
        _dbContext.Set<Budget>().Remove(budget);
    }
}
