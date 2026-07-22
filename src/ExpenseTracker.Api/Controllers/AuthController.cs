using ExpenseTracker.Api.Contracts.Authentication;
using ExpenseTracker.Application.Authentication;
using ExpenseTracker.Application.Authentication.Models;
using ExpenseTracker.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;

    public AuthController(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthenticationResponse>> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authenticationService.RegisterAsync(
            new RegisterUserCommand(
                request.DisplayName,
                request.Email,
                request.Password),
            cancellationToken);

        if (result.IsSuccess)
        {
            var user = result.Value!;
            return StatusCode(
                StatusCodes.Status201Created,
                new AuthenticationResponse(
                    new UserResponse(
                        user.Id,
                        user.DisplayName,
                        user.Email)));
        }

        if (result.Errors.Any(error => error.Type == ErrorType.Conflict))
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Email is already registered."
            });
        }

        var errors = result.Errors
            .GroupBy(error => error.Code)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.Description).ToArray());

        return BadRequest(new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred."
        });
    }
}
