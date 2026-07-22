using ExpenseTracker.Application.Authentication.Models;
using ExpenseTracker.Application.Common;

namespace ExpenseTracker.Application.Abstractions.Authentication;

public interface IIdentityService
{
    Task<UserResult?> GetUserAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<Result<UserResult>> LoginAsync(
        LoginUserCommand command,
        CancellationToken cancellationToken);

    Task LogoutAsync(CancellationToken cancellationToken);

    Task<Result<UserResult>> RegisterAsync(
        RegisterUserCommand command,
        CancellationToken cancellationToken);
}
