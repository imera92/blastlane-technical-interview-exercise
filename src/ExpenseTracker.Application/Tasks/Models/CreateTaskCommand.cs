namespace ExpenseTracker.Application.Tasks.Models;

public sealed record CreateTaskCommand(
    string Title,
    string? Description,
    string Status,
    DateOnly DueDate);
