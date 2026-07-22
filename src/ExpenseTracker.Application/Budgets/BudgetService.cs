using ExpenseTracker.Application.Abstractions.Persistence;
using ExpenseTracker.Application.Abstractions.Security;
using ExpenseTracker.Application.Budgets.Models;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Domain.Budgets;
using ExpenseTracker.Domain.Common;

namespace ExpenseTracker.Application.Budgets;

public class BudgetService : IBudgetService
{
    private readonly ICurrentUser _currentUser;
    private readonly IBudgetRepository _budgetRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public BudgetService(
        ICurrentUser currentUser,
        IBudgetRepository budgetRepository,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _currentUser = currentUser;
        _budgetRepository = budgetRepository;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<Result<BudgetResult>> CreateAsync(
        CreateBudgetCommand command,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return Result<BudgetResult>.Failure(
                new Error(
                    "Unauthorized",
                    "Authentication is required.",
                    ErrorType.Unauthorized));
        }

        Budget budget;

        try
        {
            budget = new Budget(
                _currentUser.UserId,
                command.Name,
                command.StartingBalance,
                _timeProvider.GetUtcNow());
        }
        catch (DomainValidationException exception)
        {
            return Result<BudgetResult>.Failure(
                new Error(
                    "BudgetValidation",
                    exception.Message,
                    ErrorType.Validation));
        }

        await _budgetRepository.AddAsync(budget, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<BudgetResult>.Success(
            new BudgetResult(
                budget.Id,
                budget.Name,
                budget.StartingBalance,
                budget.CurrentBalance,
                budget.CreatedAtUtc));
    }
}
