using ExpenseTracker.Application.Authentication.Models;
using ExpenseTracker.Application.Common;

namespace ExpenseTracker.Application.Authentication;

public interface IAuthenticationService
{
    Task<Result<UserResult>> RegisterAsync(
        RegisterUserCommand command,
        CancellationToken cancellationToken);
}
