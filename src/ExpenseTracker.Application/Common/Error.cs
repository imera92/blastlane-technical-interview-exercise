namespace ExpenseTracker.Application.Common;

public sealed record Error(
    string Code,
    string Description,
    ErrorType Type);
