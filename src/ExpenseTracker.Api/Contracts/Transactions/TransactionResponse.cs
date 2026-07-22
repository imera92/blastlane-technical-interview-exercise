namespace ExpenseTracker.Api.Contracts.Transactions;

public sealed record TransactionResponse(long Id, string Name, decimal Amount, DateOnly Date, DateTimeOffset CreatedAtUtc);
