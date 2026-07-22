namespace ExpenseTracker.Application.Transactions.Models;

public sealed record GroupedTransactionsResult(
    long BudgetId,
    IReadOnlyList<TransactionGroupResult> TransactionGroups);
