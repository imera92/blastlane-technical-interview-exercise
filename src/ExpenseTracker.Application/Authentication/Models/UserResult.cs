namespace ExpenseTracker.Application.Authentication.Models;

public sealed record UserResult(
    Guid Id,
    string DisplayName,
    string Email);
