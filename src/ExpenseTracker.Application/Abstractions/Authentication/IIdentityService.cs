using ExpenseTracker.Application.Authentication.Models;
using ExpenseTracker.Application.Common;

namespace ExpenseTracker.Application.Abstractions.Authentication;

public interface IIdentityService
{
    Task<Result<UserResult>> RegisterAsync(
        RegisterUserCommand command,
        CancellationToken cancellationToken);
}
