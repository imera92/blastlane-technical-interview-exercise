using ExpenseTracker.Application.Abstractions.Authentication;
using ExpenseTracker.Application.Abstractions.Security;
using ExpenseTracker.Application.Authentication.Models;
using ExpenseTracker.Application.Common;

namespace ExpenseTracker.Application.Authentication;

public class AuthenticationService : IAuthenticationService
{
    private readonly IIdentityService _identityService;
    private readonly ICurrentUser _currentUser;

    public AuthenticationService(
        IIdentityService identityService,
        ICurrentUser currentUser)
    {
        _identityService = identityService;
        _currentUser = currentUser;
    }

    public async Task<Result<UserResult>> GetCurrentUserAsync(
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return Unauthorized();
        }

        var user = await _identityService.GetUserAsync(
            _currentUser.UserId,
            cancellationToken);

        return user is null
            ? Unauthorized()
            : Result<UserResult>.Success(user);
    }

    public Task<Result<UserResult>> LoginAsync(
        LoginUserCommand command,
        CancellationToken cancellationToken)
    {
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
            Email = command.Email.Trim()
        };

        return _identityService.LoginAsync(
            normalizedCommand,
            cancellationToken);
    }

    public Task LogoutAsync(CancellationToken cancellationToken)
    {
        return _identityService.LogoutAsync(cancellationToken);
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

    private static Result<UserResult> Unauthorized()
    {
        return Result<UserResult>.Failure(
            new Error(
                "Unauthorized",
                "Authentication is required.",
                ErrorType.Unauthorized));
    }
}
