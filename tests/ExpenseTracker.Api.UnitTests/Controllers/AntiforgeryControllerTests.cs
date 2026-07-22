using ExpenseTracker.Api.Controllers;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ExpenseTracker.Api.UnitTests.Controllers;

public sealed class AntiforgeryControllerTests
{
    [Fact]
    public void Get_IssuesAngularReadableRequestTokenCookie()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddDataProtection();
        serviceCollection.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");
        using var services = serviceCollection.BuildServiceProvider();
        var httpContext = new DefaultHttpContext
        {
            RequestServices = services
        };
        var controller = new AntiforgeryController(
            services.GetRequiredService<IAntiforgery>())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };

        var result = controller.Get();

        Assert.IsType<NoContentResult>(result);
        Assert.Contains(
            httpContext.Response.Headers.SetCookie,
            value => value!.StartsWith("XSRF-TOKEN=", StringComparison.Ordinal));
    }

    [Fact]
    public void Get_IsAnonymous()
    {
        var attributes = typeof(AntiforgeryController)
            .GetMethod(nameof(AntiforgeryController.Get))!
            .GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true);

        Assert.Single(attributes);
    }
}
