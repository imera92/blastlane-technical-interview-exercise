namespace ExpenseTracker.Application.Transactions.Models;

public sealed record TransactionResult(
    long Id,
    string Name,
    decimal Amount,
    DateOnly Date,
    DateTimeOffset CreatedAtUtc);
