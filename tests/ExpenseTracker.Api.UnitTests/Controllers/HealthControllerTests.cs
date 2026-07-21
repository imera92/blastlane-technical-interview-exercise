using ExpenseTracker.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ExpenseTracker.Api.UnitTests.Controllers;

public sealed class HealthControllerTests
{
    [Fact]
    public void Get_ReturnsOk()
    {
        var controller = new HealthController();

        var result = controller.Get();

        Assert.IsType<OkResult>(result);
    }
}
