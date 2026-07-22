namespace ExpenseTracker.Api.Contracts.Transactions;

public sealed record TransactionItemResponse(long Id, string Name, decimal Amount, DateTimeOffset CreatedAtUtc);
