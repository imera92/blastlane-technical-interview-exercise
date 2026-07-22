using System.Security.Claims;
using ExpenseTracker.Api.Security;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace ExpenseTracker.Api.UnitTests.Security;

public sealed class HttpCurrentUserTests
{
    [Fact]
    public void AuthenticatedPrincipal_WithValidUserId_ExposesUser()
    {
        var userId = Guid.NewGuid();
        var currentUser = CreateCurrentUser(
            new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, userId.ToString())],
                authenticationType: "Identity.Application"));

        Assert.True(currentUser.IsAuthenticated);
        Assert.Equal(userId, currentUser.UserId);
    }

    [Fact]
    public void UnauthenticatedPrincipal_IsNotAuthenticated()
    {
        var currentUser = CreateCurrentUser(new ClaimsIdentity());

        Assert.False(currentUser.IsAuthenticated);
        Assert.Equal(Guid.Empty, currentUser.UserId);
    }

    [Fact]
    public void AuthenticatedPrincipal_WithMalformedUserId_IsNotAuthenticated()
    {
        var currentUser = CreateCurrentUser(
            new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, "not-a-guid")],
                authenticationType: "Identity.Application"));

        Assert.False(currentUser.IsAuthenticated);
        Assert.Equal(Guid.Empty, currentUser.UserId);
    }

    private static HttpCurrentUser CreateCurrentUser(ClaimsIdentity identity)
    {
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };
        var accessor = new HttpContextAccessor
        {
            HttpContext = context
        };

        return new HttpCurrentUser(accessor);
    }
}
