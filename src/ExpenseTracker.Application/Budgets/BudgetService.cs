using ExpenseTracker.Application.Abstractions.Persistence;
using ExpenseTracker.Application.Abstractions.Security;
using ExpenseTracker.Application.Budgets.Models;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.Transactions.Models;
using ExpenseTracker.Domain.Budgets;
using ExpenseTracker.Domain.Common;
using ExpenseTracker.Domain.Transactions;

namespace ExpenseTracker.Application.Budgets;

public class BudgetService : IBudgetService
{
    private readonly ICurrentUser _currentUser;
    private readonly IBudgetRepository _budgetRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public BudgetService(ICurrentUser currentUser, IBudgetRepository budgetRepository, IUnitOfWork unitOfWork, TimeProvider timeProvider)
    {
        _currentUser = currentUser;
        _budgetRepository = budgetRepository;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<Result<BudgetResult>> CreateAsync(CreateBudgetCommand command, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized<BudgetResult>();

        Budget budget;
        try
        {
            budget = new Budget(_currentUser.UserId, command.Name, command.StartingBalance, _timeProvider.GetUtcNow());
        }
        catch (DomainValidationException exception)
        {
            return Validation<BudgetResult>("BudgetValidation", exception.Message);
        }

        await _budgetRepository.AddAsync(budget, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<BudgetResult>.Success(MapSummary(budget));
    }

    public async Task<Result<IReadOnlyList<BudgetResult>>> ListAsync(CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized<IReadOnlyList<BudgetResult>>();
        var budgets = await _budgetRepository.ListForUserAsync(_currentUser.UserId, cancellationToken);
        var results = budgets.OrderBy(x => x.CreatedAtUtc).ThenBy(x => x.Id).Select(MapSummary).ToArray();
        return Result<IReadOnlyList<BudgetResult>>.Success(results);
    }

    public async Task<Result<BudgetDetailsResult>> GetAsync(long budgetId, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized<BudgetDetailsResult>();
        var budget = await _budgetRepository.GetByIdForUserAsync(budgetId, _currentUser.UserId, cancellationToken);
        return budget is null ? NotFound<BudgetDetailsResult>() : Result<BudgetDetailsResult>.Success(MapDetails(budget));
    }

    public async Task<Result<BudgetResult>> UpdateAsync(long budgetId, UpdateBudgetCommand command, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized<BudgetResult>();
        var budget = await _budgetRepository.GetByIdForUserAsync(budgetId, _currentUser.UserId, cancellationToken);
        if (budget is null) return NotFound<BudgetResult>();

        try
        {
            var validated = new Budget(budget.UserId, command.Name, command.StartingBalance, budget.CreatedAtUtc);
            budget.Rename(validated.Name);
            budget.UpdateStartingBalance(validated.StartingBalance);
        }
        catch (DomainValidationException exception)
        {
            return Validation<BudgetResult>("BudgetValidation", exception.Message);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<BudgetResult>.Success(MapSummary(budget));
    }

    public async Task<Result<bool>> DeleteAsync(long budgetId, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized<bool>();
        var budget = await _budgetRepository.GetByIdForUserAsync(budgetId, _currentUser.UserId, cancellationToken);
        if (budget is null) return NotFound<bool>();
        _budgetRepository.Remove(budget);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }

    private static BudgetResult MapSummary(Budget budget) =>
        new(budget.Id, budget.Name, budget.StartingBalance, budget.CurrentBalance, budget.CreatedAtUtc);

    private static BudgetDetailsResult MapDetails(Budget budget) =>
        new(budget.Id, budget.Name, budget.StartingBalance, budget.CurrentBalance, budget.CreatedAtUtc, Group(budget.Transactions));

    internal static IReadOnlyList<TransactionGroupResult> Group(IEnumerable<BudgetTransaction> transactions) =>
        transactions.OrderBy(x => x.Date).ThenBy(x => x.CreatedAtUtc).ThenBy(x => x.Id)
            .GroupBy(x => x.Date)
            .Select(group => new TransactionGroupResult(group.Key, group.Select(MapTransaction).ToArray()))
            .ToArray();

    internal static TransactionResult MapTransaction(BudgetTransaction transaction) =>
        new(transaction.Id, transaction.Name, transaction.Amount, transaction.Date, transaction.CreatedAtUtc);

    private static Result<T> Unauthorized<T>() => Result<T>.Failure(new Error("Unauthorized", "Authentication is required.", ErrorType.Unauthorized));
    private static Result<T> NotFound<T>() => Result<T>.Failure(new Error("NotFound", "The requested resource was not found.", ErrorType.NotFound));
    private static Result<T> Validation<T>(string code, string message) => Result<T>.Failure(new Error(code, message, ErrorType.Validation));
}
