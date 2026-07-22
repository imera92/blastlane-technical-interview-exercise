namespace ExpenseTracker.Application.Authentication.Models;

public sealed record RegisterUserCommand(
    string DisplayName,
    string Email,
    string Password);
