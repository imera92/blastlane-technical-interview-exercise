using ExpenseTracker.Application.Budgets.Models;
using ExpenseTracker.Application.Common;

namespace ExpenseTracker.Application.Budgets;

public interface IBudgetService
{
    Task<Result<BudgetResult>> CreateAsync(
        CreateBudgetCommand command,
        CancellationToken cancellationToken);
}
