namespace ExpenseTracker.Application.Authentication.Models;

public sealed record LoginUserCommand(
    string Email,
    string Password);
