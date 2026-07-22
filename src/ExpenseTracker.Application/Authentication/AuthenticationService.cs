using ExpenseTracker.Application.Abstractions.Authentication;
using ExpenseTracker.Application.Authentication.Models;
using ExpenseTracker.Application.Common;

namespace ExpenseTracker.Application.Authentication;

public class AuthenticationService : IAuthenticationService
{
    private readonly IIdentityService _identityService;
    public AuthenticationService(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public Task<Result<UserResult>> RegisterAsync(
        RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.DisplayName))
        {
            return Task.FromResult(Result<UserResult>.Failure(
                new Error(
                    "DisplayNameRequired",
                    "Display name is required.",
                    ErrorType.Validation)));
        }

        if (command.DisplayName.Trim().Length > 100)
        {
            return Task.FromResult(Result<UserResult>.Failure(
                new Error(
                    "DisplayNameTooLong",
                    "Display name cannot exceed 100 characters.",
                    ErrorType.Validation)));
        }

        if (string.IsNullOrWhiteSpace(command.Email))
        {
            return Task.FromResult(Result<UserResult>.Failure(
                new Error(
                    "EmailRequired",
                    "Email is required.",
                    ErrorType.Validation)));
        }

        if (string.IsNullOrWhiteSpace(command.Password))
        {
            return Task.FromResult(Result<UserResult>.Failure(
                new Error(
                    "PasswordRequired",
                    "Password is required.",
                    ErrorType.Validation)));
        }

        var normalizedCommand = command with
        {
            DisplayName = command.DisplayName.Trim(),
            Email = command.Email.Trim()
        };

        return _identityService.RegisterAsync(
            normalizedCommand,
            cancellationToken);
    }
}
