using ExpenseTracker.Api.Contracts.Transactions;

namespace ExpenseTracker.Api.Contracts.Budgets;

public sealed record BudgetDetailsResponse(
    long Id,
    string Name,
    decimal StartingBalance,
    decimal CurrentBalance,
    DateTimeOffset CreatedAtUtc,
    IReadOnlyList<TransactionGroupResponse> TransactionGroups);
