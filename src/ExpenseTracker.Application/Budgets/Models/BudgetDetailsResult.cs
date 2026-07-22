using ExpenseTracker.Application.Transactions.Models;

namespace ExpenseTracker.Application.Budgets.Models;

public sealed record BudgetDetailsResult(
    long Id,
    string Name,
    decimal StartingBalance,
    decimal CurrentBalance,
    DateTimeOffset CreatedAtUtc,
    IReadOnlyList<TransactionGroupResult> TransactionGroups);
