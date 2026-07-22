using ExpenseTracker.Application.Abstractions.Persistence;
using ExpenseTracker.Application.Abstractions.Security;
using ExpenseTracker.Application.Budgets;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.Transactions.Models;
using ExpenseTracker.Domain.Common;
using ExpenseTracker.Domain.Transactions;

namespace ExpenseTracker.Application.Transactions;

public class TransactionService : ITransactionService
{
    private readonly ICurrentUser _currentUser;
    private readonly IBudgetRepository _budgetRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public TransactionService(ICurrentUser currentUser, IBudgetRepository budgetRepository, ITransactionRepository transactionRepository, IUnitOfWork unitOfWork, TimeProvider timeProvider)
    {
        _currentUser = currentUser;
        _budgetRepository = budgetRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<Result<TransactionResult>> CreateAsync(long budgetId, CreateTransactionCommand command, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized<TransactionResult>();
        var budget = await _budgetRepository.GetByIdForUserAsync(budgetId, _currentUser.UserId, cancellationToken);
        if (budget is null) return NotFound<TransactionResult>();
        BudgetTransaction transaction;
        try
        {
            transaction = budget.AddTransaction(command.Name, command.Amount, command.Date, _timeProvider.GetUtcNow());
        }
        catch (DomainValidationException exception)
        {
            return Validation<TransactionResult>(exception.Message);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<TransactionResult>.Success(BudgetService.MapTransaction(transaction));
    }

    public async Task<Result<GroupedTransactionsResult>> ListAsync(long budgetId, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized<GroupedTransactionsResult>();
        if (!await _budgetRepository.ExistsForUserAsync(budgetId, _currentUser.UserId, cancellationToken)) return NotFound<GroupedTransactionsResult>();
        var transactions = await _transactionRepository.ListForBudgetAndUserAsync(budgetId, _currentUser.UserId, cancellationToken);
        return Result<GroupedTransactionsResult>.Success(new GroupedTransactionsResult(budgetId, BudgetService.Group(transactions)));
    }

    public async Task<Result<TransactionResult>> GetAsync(long budgetId, long transactionId, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized<TransactionResult>();
        var transaction = await Find(budgetId, transactionId, cancellationToken);
        return transaction is null ? NotFound<TransactionResult>() : Result<TransactionResult>.Success(BudgetService.MapTransaction(transaction));
    }

    public async Task<Result<TransactionResult>> UpdateAsync(long budgetId, long transactionId, UpdateTransactionCommand command, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized<TransactionResult>();
        var transaction = await Find(budgetId, transactionId, cancellationToken);
        if (transaction is null) return NotFound<TransactionResult>();
        try
        {
            transaction.Update(command.Name, command.Amount, command.Date);
        }
        catch (DomainValidationException exception)
        {
            return Validation<TransactionResult>(exception.Message);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<TransactionResult>.Success(BudgetService.MapTransaction(transaction));
    }

    public async Task<Result<bool>> DeleteAsync(long budgetId, long transactionId, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized<bool>();
        var transaction = await Find(budgetId, transactionId, cancellationToken);
        if (transaction is null) return NotFound<bool>();
        _transactionRepository.Remove(transaction);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }

    private Task<BudgetTransaction?> Find(long budgetId, long transactionId, CancellationToken cancellationToken) =>
        _transactionRepository.GetByIdForUserAsync(budgetId, transactionId, _currentUser.UserId, cancellationToken);

    private static Result<T> Unauthorized<T>() => Result<T>.Failure(new Error("Unauthorized", "Authentication is required.", ErrorType.Unauthorized));
    private static Result<T> NotFound<T>() => Result<T>.Failure(new Error("NotFound", "The requested resource was not found.", ErrorType.NotFound));
    private static Result<T> Validation<T>(string message) => Result<T>.Failure(new Error("TransactionValidation", message, ErrorType.Validation));
}
