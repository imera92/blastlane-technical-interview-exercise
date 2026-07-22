namespace ExpenseTracker.Api.Contracts.Authentication;

public sealed record UserResponse(
    Guid Id,
    string DisplayName,
    string Email);
