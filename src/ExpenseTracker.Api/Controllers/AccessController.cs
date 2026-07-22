using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;

[ApiController]
[Route("api/access")]
public sealed class AccessController : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("public")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Public()
    {
        return Ok();
    }

    [Authorize]
    [HttpGet("protected")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Protected()
    {
        return Ok();
    }
}
