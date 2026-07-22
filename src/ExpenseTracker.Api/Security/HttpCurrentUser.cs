using System.Security.Claims;
using ExpenseTracker.Application.Abstractions.Security;

namespace ExpenseTracker.Api.Security;

public sealed class HttpCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated => TryGetUserId(out _);

    public Guid UserId
    {
        get
        {
            TryGetUserId(out var userId);
            return userId;
        }
    }

    private bool TryGetUserId(out Guid userId)
    {
        var principal = _httpContextAccessor.HttpContext?.User;

        if (principal?.Identity?.IsAuthenticated != true)
        {
            userId = Guid.Empty;
            return false;
        }

        var claimValue = principal
            .FindFirst(ClaimTypes.NameIdentifier)?
            .Value;

        return Guid.TryParse(claimValue, out userId);
    }
}
