using System.ComponentModel.DataAnnotations;
using ExpenseTracker.Api.Contracts.Authentication;
using ExpenseTracker.Api.Controllers;
using ExpenseTracker.Application.Authentication;
using ExpenseTracker.Application.Authentication.Models;
using ExpenseTracker.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ExpenseTracker.Api.UnitTests.Controllers;

public sealed class AuthControllerTests
{
    [Fact]
    public async Task Register_WithSuccessfulRegistration_ReturnsCreatedUser()
    {
        var user = new UserResult(Guid.NewGuid(), "Iván", "ivan@example.com");
        var authenticationService = new StubAuthenticationService(
            Result<UserResult>.Success(user));
        var controller = new AuthController(authenticationService);
        var request = new RegisterRequest
        {
            DisplayName = "Iván",
            Email = "ivan@example.com",
            Password = "Password123"
        };

        var result = await controller.Register(request, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status201Created, objectResult.StatusCode);
        var response = Assert.IsType<AuthenticationResponse>(objectResult.Value);
        Assert.Equal(user.Id, response.User.Id);
        Assert.Equal(user.DisplayName, response.User.DisplayName);
        Assert.Equal(user.Email, response.User.Email);
        Assert.Equal(
            new RegisterUserCommand(
                request.DisplayName,
                request.Email,
                request.Password),
            authenticationService.ReceivedCommand);
    }

    [Fact]
    public async Task Register_WithValidationFailure_ReturnsValidationProblem()
    {
        var error = new Error(
            "PasswordRequiresDigit",
            "Passwords must have at least one digit.",
            ErrorType.Validation);
        var authenticationService = new StubAuthenticationService(
            Result<UserResult>.Failure(error));
        var controller = new AuthController(authenticationService);

        var result = await controller.Register(CreateRequest(), CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var problem = Assert.IsType<ValidationProblemDetails>(badRequest.Value);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
        Assert.Equal(error.Description, Assert.Single(problem.Errors[error.Code]));
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflictProblem()
    {
        var authenticationService = new StubAuthenticationService(
            Result<UserResult>.Failure(
                new Error(
                    "DuplicateEmail",
                    "Email is already registered.",
                    ErrorType.Conflict)));
        var controller = new AuthController(authenticationService);

        var result = await controller.Register(CreateRequest(), CancellationToken.None);

        var conflict = Assert.IsType<ConflictObjectResult>(result.Result);
        var problem = Assert.IsType<ProblemDetails>(conflict.Value);
        Assert.Equal(StatusCodes.Status409Conflict, problem.Status);
        Assert.Equal("Email is already registered.", problem.Title);
    }

    [Fact]
    public void RegisterRequest_WithInvalidFields_FailsDataAnnotationValidation()
    {
        var request = new RegisterRequest
        {
            DisplayName = new string('a', 101),
            Email = "not-an-email",
            Password = string.Empty
        };
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            validationResults,
            validateAllProperties: true);

        Assert.False(isValid);
        Assert.Equal(3, validationResults.Count);
    }

    [Fact]
    public async Task Login_WithSuccessfulAuthentication_ReturnsOkUser()
    {
        var user = new UserResult(Guid.NewGuid(), "Iván", "ivan@example.com");
        var authenticationService = new StubAuthenticationService(
            Result<UserResult>.Success(user));
        var controller = new AuthController(authenticationService);
        var request = new LoginRequest
        {
            Email = "ivan@example.com",
            Password = "Password123"
        };

        var result = await controller.Login(request, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<AuthenticationResponse>(ok.Value);
        Assert.Equal(user.Id, response.User.Id);
        Assert.Equal(user.DisplayName, response.User.DisplayName);
        Assert.Equal(user.Email, response.User.Email);
        Assert.Equal(
            new LoginUserCommand(request.Email, request.Password),
            authenticationService.ReceivedLoginCommand);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorizedProblem()
    {
        var authenticationService = new StubAuthenticationService(
            Result<UserResult>.Failure(
                new Error(
                    "InvalidCredentials",
                    "Invalid email or password.",
                    ErrorType.Unauthorized)));
        var controller = new AuthController(authenticationService);

        var result = await controller.Login(
            CreateLoginRequest(),
            CancellationToken.None);

        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        var problem = Assert.IsType<ProblemDetails>(unauthorized.Value);
        Assert.Equal(StatusCodes.Status401Unauthorized, problem.Status);
        Assert.Equal("Invalid email or password.", problem.Title);
    }

    [Fact]
    public async Task Login_WithValidationFailure_ReturnsValidationProblem()
    {
        var error = new Error(
            "EmailRequired",
            "Email is required.",
            ErrorType.Validation);
        var authenticationService = new StubAuthenticationService(
            Result<UserResult>.Failure(error));
        var controller = new AuthController(authenticationService);

        var result = await controller.Login(
            CreateLoginRequest(),
            CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var problem = Assert.IsType<ValidationProblemDetails>(badRequest.Value);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
        Assert.Equal(error.Description, Assert.Single(problem.Errors[error.Code]));
    }

    [Fact]
    public void LoginRequest_WithInvalidFields_FailsDataAnnotationValidation()
    {
        var request = new LoginRequest
        {
            Email = "not-an-email",
            Password = string.Empty
        };
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            validationResults,
            validateAllProperties: true);

        Assert.False(isValid);
        Assert.Equal(2, validationResults.Count);
    }

    [Fact]
    public async Task Me_WithAuthenticatedUser_ReturnsOkUser()
    {
        var user = new UserResult(Guid.NewGuid(), "Iván", "ivan@example.com");
        var authenticationService = new StubAuthenticationService(
            Result<UserResult>.Success(user));
        var controller = new AuthController(authenticationService);

        var result = await controller.Me(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<AuthenticationResponse>(ok.Value);
        Assert.Equal(user.Id, response.User.Id);
        Assert.Equal(user.DisplayName, response.User.DisplayName);
        Assert.Equal(user.Email, response.User.Email);
    }

    [Fact]
    public async Task Me_WithUnauthorizedResult_ReturnsUnauthorizedProblem()
    {
        var authenticationService = new StubAuthenticationService(
            Result<UserResult>.Failure(
                new Error(
                    "Unauthorized",
                    "Authentication is required.",
                    ErrorType.Unauthorized)));
        var controller = new AuthController(authenticationService);

        var result = await controller.Me(CancellationToken.None);

        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        var problem = Assert.IsType<ProblemDetails>(unauthorized.Value);
        Assert.Equal(StatusCodes.Status401Unauthorized, problem.Status);
        Assert.Equal("Authentication is required.", problem.Title);
    }

    [Fact]
    public void AnonymousAuthenticationActions_HaveAllowAnonymousMetadata()
    {
        var controllerType = typeof(AuthController);

        Assert.NotEmpty(controllerType
            .GetMethod(nameof(AuthController.Register))!
            .GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true));
        Assert.NotEmpty(controllerType
            .GetMethod(nameof(AuthController.Login))!
            .GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true));
    }

    [Fact]
    public void Me_HasAuthorizeMetadata()
    {
        var attributes = typeof(AuthController)
            .GetMethod(nameof(AuthController.Me))!
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true);

        Assert.NotEmpty(attributes);
    }

    private static RegisterRequest CreateRequest()
    {
        return new RegisterRequest
        {
            DisplayName = "Iván",
            Email = "ivan@example.com",
            Password = "Password123"
        };
    }

    private static LoginRequest CreateLoginRequest()
    {
        return new LoginRequest
        {
            Email = "ivan@example.com",
            Password = "Password123"
        };
    }

    private sealed class StubAuthenticationService : IAuthenticationService
    {
        private readonly Result<UserResult> _result;

        public StubAuthenticationService(Result<UserResult> result)
        {
            _result = result;
        }

        public RegisterUserCommand? ReceivedCommand { get; private set; }
        public LoginUserCommand? ReceivedLoginCommand { get; private set; }

        public Task<Result<UserResult>> GetCurrentUserAsync(
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_result);
        }

        public Task<Result<UserResult>> LoginAsync(
            LoginUserCommand command,
            CancellationToken cancellationToken)
        {
            ReceivedLoginCommand = command;
            return Task.FromResult(_result);
        }

        public Task<Result<UserResult>> RegisterAsync(
            RegisterUserCommand command,
            CancellationToken cancellationToken)
        {
            ReceivedCommand = command;
            return Task.FromResult(_result);
        }
    }
}
