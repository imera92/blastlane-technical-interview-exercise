namespace ExpenseTracker.Application.Budgets.Models;

public sealed record CreateBudgetCommand(
    string Name,
    decimal StartingBalance);
