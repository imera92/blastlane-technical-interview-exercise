namespace ExpenseTracker.Api.Contracts.Transactions;

public sealed record GroupedTransactionsResponse(long BudgetId, IReadOnlyList<TransactionGroupResponse> TransactionGroups);
