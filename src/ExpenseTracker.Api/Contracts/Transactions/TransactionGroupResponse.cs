namespace ExpenseTracker.Api.Contracts.Transactions;

public sealed record TransactionGroupResponse(DateOnly Date, IReadOnlyList<TransactionItemResponse> Transactions);
