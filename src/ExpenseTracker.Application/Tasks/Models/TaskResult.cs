namespace ExpenseTracker.Application.Tasks.Models;

public sealed record TaskResult(
    long Id,
    string Title,
    string Description,
    string Status,
    DateOnly DueDate);
