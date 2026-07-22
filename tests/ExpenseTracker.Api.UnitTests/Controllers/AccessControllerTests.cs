using ExpenseTracker.Api.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ExpenseTracker.Api.UnitTests.Controllers;

public sealed class AccessControllerTests
{
    [Fact]
    public void Public_ReturnsOk()
    {
        var controller = new AccessController();

        var result = controller.Public();

        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public void Protected_ReturnsOk()
    {
        var controller = new AccessController();

        var result = controller.Protected();

        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public void Public_HasAllowAnonymousMetadata()
    {
        var attributes = typeof(AccessController)
            .GetMethod(nameof(AccessController.Public))!
            .GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true);

        Assert.NotEmpty(attributes);
    }

    [Fact]
    public void Protected_HasAuthorizeMetadata()
    {
        var attributes = typeof(AccessController)
            .GetMethod(nameof(AccessController.Protected))!
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true);

        Assert.NotEmpty(attributes);
    }
}
