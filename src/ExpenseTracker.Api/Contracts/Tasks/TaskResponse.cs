namespace ExpenseTracker.Api.Contracts.Tasks;

public sealed record TaskResponse(
    long Id,
    string Title,
    string Description,
    string Status,
    DateOnly DueDate);
