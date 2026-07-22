using ExpenseTracker.Application.Authentication.Models;
using ExpenseTracker.Application.Common;

namespace ExpenseTracker.Application.Authentication;

public interface IAuthenticationService
{
    Task<Result<UserResult>> GetCurrentUserAsync(
        CancellationToken cancellationToken);

    Task<Result<UserResult>> LoginAsync(
        LoginUserCommand command,
        CancellationToken cancellationToken);

    Task<Result<UserResult>> RegisterAsync(
        RegisterUserCommand command,
        CancellationToken cancellationToken);
}
