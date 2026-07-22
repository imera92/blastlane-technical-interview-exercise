using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;

[ApiController]
[Route("api/auth/antiforgery")]
public sealed class AntiforgeryController : ControllerBase
{
    private readonly IAntiforgery _antiforgery;

    public AntiforgeryController(IAntiforgery antiforgery)
    {
        _antiforgery = antiforgery;
    }

    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult Get()
    {
        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);

        Response.Cookies.Append(
            "XSRF-TOKEN",
            tokens.RequestToken!,
            new CookieOptions
            {
                HttpOnly = false,
                IsEssential = true,
                Path = "/",
                SameSite = SameSiteMode.Strict,
                Secure = Request.IsHttps
            });

        return NoContent();
    }
}
