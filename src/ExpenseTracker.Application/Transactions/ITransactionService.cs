using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.Transactions.Models;

namespace ExpenseTracker.Application.Transactions;

public interface ITransactionService
{
    Task<Result<TransactionResult>> CreateAsync(long budgetId, CreateTransactionCommand command, CancellationToken cancellationToken);
    Task<Result<bool>> DeleteAsync(long budgetId, long transactionId, CancellationToken cancellationToken);
    Task<Result<TransactionResult>> GetAsync(long budgetId, long transactionId, CancellationToken cancellationToken);
    Task<Result<GroupedTransactionsResult>> ListAsync(long budgetId, CancellationToken cancellationToken);
    Task<Result<TransactionResult>> UpdateAsync(long budgetId, long transactionId, UpdateTransactionCommand command, CancellationToken cancellationToken);
}
