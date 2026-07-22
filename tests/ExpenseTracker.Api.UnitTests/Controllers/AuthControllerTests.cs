using System.ComponentModel.DataAnnotations;
using ExpenseTracker.Api.Contracts.Authentication;
using ExpenseTracker.Api.Controllers;
using ExpenseTracker.Application.Authentication;
using ExpenseTracker.Application.Authentication.Models;
using ExpenseTracker.Application.Common;
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

    private static RegisterRequest CreateRequest()
    {
        return new RegisterRequest
        {
            DisplayName = "Iván",
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

        public Task<Result<UserResult>> RegisterAsync(
            RegisterUserCommand command,
            CancellationToken cancellationToken)
        {
            ReceivedCommand = command;
            return Task.FromResult(_result);
        }
    }
}
