namespace ExpenseTracker.Application.Tasks.Models;

public sealed record UpdateTaskCommand(
    string Title,
    string? Description,
    string Status,
    DateOnly DueDate);
