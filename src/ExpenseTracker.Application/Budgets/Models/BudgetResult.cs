namespace ExpenseTracker.Application.Budgets.Models;

public sealed record BudgetResult(
    long Id,
    string Name,
    decimal StartingBalance,
    decimal CurrentBalance,
    DateTimeOffset CreatedAtUtc);
