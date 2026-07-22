namespace ExpenseTracker.Api.Contracts.Budgets;

public sealed record BudgetSummaryResponse(
    long Id,
    string Name,
    decimal StartingBalance,
    decimal CurrentBalance,
    DateTimeOffset CreatedAtUtc);
