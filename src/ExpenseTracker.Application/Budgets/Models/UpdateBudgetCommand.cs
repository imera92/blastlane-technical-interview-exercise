namespace ExpenseTracker.Application.Budgets.Models;

public sealed record UpdateBudgetCommand(string Name, decimal StartingBalance);
