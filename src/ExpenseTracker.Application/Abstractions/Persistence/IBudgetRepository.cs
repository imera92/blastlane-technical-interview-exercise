using ExpenseTracker.Domain.Budgets;

namespace ExpenseTracker.Application.Abstractions.Persistence;

public interface IBudgetRepository
{
    Task<bool> ExistsForUserAsync(
        long budgetId,
        Guid userId,
        CancellationToken cancellationToken);

    Task<Budget?> GetByIdForUserAsync(
        long budgetId,
        Guid userId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Budget>> ListForUserAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task AddAsync(
        Budget budget,
        CancellationToken cancellationToken);

    void Remove(Budget budget);
}
