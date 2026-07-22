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
    private readonly SignInManager<ApplicationUser> _signInManager;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<UserResult?> GetUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await _userManager.FindByIdAsync(userId.ToString());

        return user is null ? null : MapUser(user);
    }

    public async Task<Result<UserResult>> LoginAsync(
        LoginUserCommand command,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await _userManager.FindByEmailAsync(command.Email);

        if (user is null)
        {
            return InvalidCredentials();
        }

        var signInResult = await _signInManager.PasswordSignInAsync(
            user,
            command.Password,
            isPersistent: false,
            lockoutOnFailure: false);

        return signInResult.Succeeded
            ? Result<UserResult>.Success(MapUser(user))
            : InvalidCredentials();
    }

    public Task LogoutAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return _signInManager.SignOutAsync();
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

    private static UserResult MapUser(ApplicationUser user)
    {
        return new UserResult(
            user.Id,
            user.DisplayName,
            user.Email!);
    }

    private static Result<UserResult> InvalidCredentials()
    {
        return Result<UserResult>.Failure(
            new Error(
                "InvalidCredentials",
                "Invalid email or password.",
                ErrorType.Unauthorized));
    }
}
