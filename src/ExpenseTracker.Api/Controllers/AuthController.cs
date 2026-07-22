using ExpenseTracker.Api.Contracts.Authentication;
using ExpenseTracker.Application.Authentication;
using ExpenseTracker.Application.Authentication.Models;
using ExpenseTracker.Application.Common;
using Microsoft.AspNetCore.Authorization;
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

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthenticationResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authenticationService.LoginAsync(
            new LoginUserCommand(request.Email, request.Password),
            cancellationToken);

        if (result.IsFailure)
        {
            if (result.Errors.Any(error => error.Type == ErrorType.Unauthorized))
            {
                return Unauthorized(new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Invalid email or password."
                });
            }

            return BadRequest(ToValidationProblem(result.Errors));
        }

        var user = result.Value!;

        return Ok(ToResponse(user));
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthenticationResponse>> Me(
        CancellationToken cancellationToken)
    {
        var result = await _authenticationService.GetCurrentUserAsync(
            cancellationToken);

        if (result.IsFailure)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Authentication is required."
            });
        }

        return Ok(ToResponse(result.Value!));
    }

    [AllowAnonymous]
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

        return BadRequest(ToValidationProblem(result.Errors));
    }

    private static ValidationProblemDetails ToValidationProblem(
        IReadOnlyList<Error> resultErrors)
    {
        var errors = resultErrors
            .GroupBy(error => error.Code)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.Description).ToArray());

        return new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred."
        };
    }

    private static AuthenticationResponse ToResponse(UserResult user)
    {
        return new AuthenticationResponse(
            new UserResponse(
                user.Id,
                user.DisplayName,
                user.Email));
    }
}
