using ExpenseTracker.Application.Abstractions.Authentication;
using ExpenseTracker.Application.Authentication.Models;
using ExpenseTracker.Application.Common;
using Microsoft.AspNetCore.Identity;

namespace ExpenseTracker.Infrastructure.Authentication;

internal sealed class IdentityService : IIdentityService
{
    private const string DuplicateEmailCode = "DuplicateEmail";
    private const string DuplicateUserNameCode = "DuplicateUserName";

    private readonly UserManager<ApplicationUser> _userManager;
    public IdentityService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<UserResult>> RegisterAsync(
        RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            DisplayName = command.DisplayName,
            Email = command.Email,
            UserName = command.Email,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        var identityResult = await _userManager.CreateAsync(user, command.Password);

        if (identityResult.Succeeded)
        {
            return Result<UserResult>.Success(
                new UserResult(
                    user.Id,
                    user.DisplayName,
                    command.Email));
        }

        var errors = identityResult.Errors
            .Select(MapError)
            .ToArray();

        if (errors.Length == 0)
        {
            errors =
            [
                new Error(
                    "IdentityRegistrationFailed",
                    "Registration could not be completed.",
                    ErrorType.Validation)
            ];
        }

        return Result<UserResult>.Failure(errors);
    }

    private static Error MapError(IdentityError error)
    {
        var errorType = error.Code is DuplicateEmailCode or DuplicateUserNameCode
            ? ErrorType.Conflict
            : ErrorType.Validation;

        return new Error(error.Code, error.Description, errorType);
    }
}
