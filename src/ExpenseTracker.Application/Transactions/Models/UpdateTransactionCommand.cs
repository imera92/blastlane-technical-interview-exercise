namespace ExpenseTracker.Application.Transactions.Models;

public sealed record UpdateTransactionCommand(string Name, decimal Amount, DateOnly Date);
