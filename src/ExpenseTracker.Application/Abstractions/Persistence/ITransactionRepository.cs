using ExpenseTracker.Domain.Transactions;

namespace ExpenseTracker.Application.Abstractions.Persistence;

public interface ITransactionRepository
{
    Task<BudgetTransaction?> GetByIdForUserAsync(
        long budgetId,
        long transactionId,
        Guid userId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<BudgetTransaction>> ListForBudgetAndUserAsync(
        long budgetId,
        Guid userId,
        CancellationToken cancellationToken);

    void Remove(BudgetTransaction transaction);
}
