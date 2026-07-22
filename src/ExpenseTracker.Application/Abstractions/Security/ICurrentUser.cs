namespace ExpenseTracker.Application.Abstractions.Security;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    Guid UserId { get; }
}
