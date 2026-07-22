namespace ExpenseTracker.Application.Transactions.Models;

public sealed record TransactionGroupResult(
    DateOnly Date,
    IReadOnlyList<TransactionResult> Transactions);
