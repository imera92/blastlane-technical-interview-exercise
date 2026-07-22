using ExpenseTracker.Application.Abstractions.Authentication;
using ExpenseTracker.Application.Authentication;
using ExpenseTracker.Application.Authentication.Models;
using ExpenseTracker.Application.Common;
using Xunit;

namespace ExpenseTracker.Application.UnitTests.Authentication;

public sealed class AuthenticationServiceTests
{
    [Fact]
    public async Task RegisterAsync_WithValidInput_CallsIdentityServiceAndReturnsUser()
    {
        var expectedUser = new UserResult(
            Guid.NewGuid(),
            "Iván",
            "ivan@example.com");
        var identityService = new RecordingIdentityService(
            Result<UserResult>.Success(expectedUser));
        var service = new AuthenticationService(identityService);
        var cancellationToken = new CancellationTokenSource().Token;
        var command = new RegisterUserCommand(
            "  Iván  ",
            "  ivan@example.com  ",
            "Password123");

        var result = await service.RegisterAsync(command, cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedUser, result.Value);
        Assert.Equal(1, identityService.CallCount);
        Assert.Equal("Iván", identityService.ReceivedCommand!.DisplayName);
        Assert.Equal("ivan@example.com", identityService.ReceivedCommand.Email);
        Assert.Equal("Password123", identityService.ReceivedCommand.Password);
        Assert.Equal(cancellationToken, identityService.ReceivedCancellationToken);
    }

    [Fact]
    public async Task RegisterAsync_WithWhitespaceDisplayName_ReturnsValidationWithoutCallingIdentity()
    {
        var identityService = new RecordingIdentityService(
            Result<UserResult>.Success(CreateUserResult()));
        var service = new AuthenticationService(identityService);
        var command = new RegisterUserCommand(
            "   ",
            "ivan@example.com",
            "Password123");

        var result = await service.RegisterAsync(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, Assert.Single(result.Errors).Type);
        Assert.Equal(0, identityService.CallCount);
    }

    [Fact]
    public async Task RegisterAsync_WithOverlongDisplayName_ReturnsValidationWithoutCallingIdentity()
    {
        var identityService = new RecordingIdentityService(
            Result<UserResult>.Success(CreateUserResult()));
        var service = new AuthenticationService(identityService);
        var command = new RegisterUserCommand(
            new string('a', 101),
            "ivan@example.com",
            "Password123");

        var result = await service.RegisterAsync(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, Assert.Single(result.Errors).Type);
        Assert.Equal(0, identityService.CallCount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RegisterAsync_WithMissingEmail_ReturnsValidationWithoutCallingIdentity(
        string? email)
    {
        var identityService = new RecordingIdentityService(
            Result<UserResult>.Success(CreateUserResult()));
        var service = new AuthenticationService(identityService);
        var command = new RegisterUserCommand(
            "Iván",
            email!,
            "Password123");

        var result = await service.RegisterAsync(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, Assert.Single(result.Errors).Type);
        Assert.Equal(0, identityService.CallCount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RegisterAsync_WithMissingPassword_ReturnsValidationWithoutCallingIdentity(
        string? password)
    {
        var identityService = new RecordingIdentityService(
            Result<UserResult>.Success(CreateUserResult()));
        var service = new AuthenticationService(identityService);
        var command = new RegisterUserCommand(
            "Iván",
            "ivan@example.com",
            password!);

        var result = await service.RegisterAsync(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, Assert.Single(result.Errors).Type);
        Assert.Equal(0, identityService.CallCount);
    }

    [Fact]
    public async Task RegisterAsync_WhenIdentityReportsDuplicateEmail_ReturnsConflict()
    {
        var expectedError = new Error(
            "DuplicateEmail",
            "Email is already registered.",
            ErrorType.Conflict);
        var identityService = new RecordingIdentityService(
            Result<UserResult>.Failure(expectedError));
        var service = new AuthenticationService(identityService);
        var command = new RegisterUserCommand(
            "Iván",
            "ivan@example.com",
            "Password123");

        var result = await service.RegisterAsync(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(expectedError, Assert.Single(result.Errors));
        Assert.Equal(1, identityService.CallCount);
    }

    [Fact]
    public async Task RegisterAsync_WhenIdentityReportsValidationErrors_ReturnsThoseErrors()
    {
        var expectedErrors = new[]
        {
            new Error(
                "PasswordRequiresUpper",
                "Passwords must have at least one uppercase character.",
                ErrorType.Validation),
            new Error(
                "PasswordRequiresDigit",
                "Passwords must have at least one digit.",
                ErrorType.Validation)
        };
        var identityService = new RecordingIdentityService(
            Result<UserResult>.Failure(expectedErrors));
        var service = new AuthenticationService(identityService);
        var command = new RegisterUserCommand(
            "Iván",
            "ivan@example.com",
            "password");

        var result = await service.RegisterAsync(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(expectedErrors, result.Errors);
        Assert.Equal(1, identityService.CallCount);
    }

    [Fact]
    public void UserResult_DoesNotExposePasswordData()
    {
        var properties = typeof(UserResult).GetProperties();

        Assert.DoesNotContain(
            properties,
            property => property.Name.Contains(
                "Password",
                StringComparison.OrdinalIgnoreCase));
    }

    private static UserResult CreateUserResult()
    {
        return new UserResult(Guid.NewGuid(), "Iván", "ivan@example.com");
    }

    private sealed class RecordingIdentityService : IIdentityService
    {
        private readonly Result<UserResult> _result;

        public RecordingIdentityService(Result<UserResult> result)
        {
            _result = result;
        }

        public int CallCount { get; private set; }
        public RegisterUserCommand? ReceivedCommand { get; private set; }
        public CancellationToken ReceivedCancellationToken { get; private set; }

        public Task<Result<UserResult>> RegisterAsync(
            RegisterUserCommand command,
            CancellationToken cancellationToken)
        {
            CallCount++;
            ReceivedCommand = command;
            ReceivedCancellationToken = cancellationToken;

            return Task.FromResult(_result);
        }
    }
}
