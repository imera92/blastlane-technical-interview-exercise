using ExpenseTracker.Application.Abstractions.Persistence;
using ExpenseTracker.Domain.Budgets;
using ExpenseTracker.Domain.Transactions;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Persistence.Repositories;

internal sealed class TransactionRepository : ITransactionRepository
{
    private readonly ExpenseTrackerDbContext _dbContext;

    public TransactionRepository(ExpenseTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<BudgetTransaction?> GetByIdForUserAsync(
        long budgetId,
        long transactionId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        return QueryForUser(budgetId, userId)
            .SingleOrDefaultAsync(
                transaction => transaction.Id == transactionId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<BudgetTransaction>> ListForBudgetAndUserAsync(
        long budgetId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await QueryForUser(budgetId, userId)
            .AsNoTracking()
            .OrderBy(transaction => transaction.Date)
            .ThenBy(transaction => transaction.CreatedAtUtc)
            .ThenBy(transaction => transaction.Id)
            .ToListAsync(cancellationToken);
    }

    public void Remove(BudgetTransaction transaction)
    {
        _dbContext.Set<BudgetTransaction>().Remove(transaction);
    }

    private IQueryable<BudgetTransaction> QueryForUser(long budgetId, Guid userId)
    {
        return
            from transaction in _dbContext.Set<BudgetTransaction>()
            join budget in _dbContext.Set<Budget>()
                on transaction.BudgetId equals budget.Id
            where transaction.BudgetId == budgetId && budget.UserId == userId
            select transaction;
    }
}
