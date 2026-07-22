using ExpenseTracker.Application.Budgets.Models;
using ExpenseTracker.Application.Common;

namespace ExpenseTracker.Application.Budgets;

public interface IBudgetService
{
    Task<Result<BudgetResult>> CreateAsync(
        CreateBudgetCommand command,
        CancellationToken cancellationToken);

    Task<Result<bool>> DeleteAsync(long budgetId, CancellationToken cancellationToken);

    Task<Result<BudgetDetailsResult>> GetAsync(
        long budgetId,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<BudgetResult>>> ListAsync(
        CancellationToken cancellationToken);

    Task<Result<BudgetResult>> UpdateAsync(
        long budgetId,
        UpdateBudgetCommand command,
        CancellationToken cancellationToken);
}
