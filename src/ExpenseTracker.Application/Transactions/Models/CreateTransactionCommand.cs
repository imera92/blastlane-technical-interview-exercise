namespace ExpenseTracker.Application.Transactions.Models;

public sealed record CreateTransactionCommand(string Name, decimal Amount, DateOnly Date);
