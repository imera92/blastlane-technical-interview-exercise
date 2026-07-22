using ExpenseTracker.Application.Abstractions.Authentication;
using ExpenseTracker.Application.Abstractions.Security;
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
        var service = CreateService(identityService);
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
        var service = CreateService(identityService);
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
        var service = CreateService(identityService);
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
        var service = CreateService(identityService);
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
        var service = CreateService(identityService);
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
        var service = CreateService(identityService);
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
        var service = CreateService(identityService);
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
    public async Task LoginAsync_WithValidCredentials_CallsIdentityServiceAndReturnsUser()
    {
        var expectedUser = CreateUserResult();
        var identityService = new RecordingIdentityService(
            Result<UserResult>.Success(expectedUser));
        var service = CreateService(identityService);
        var cancellationToken = new CancellationTokenSource().Token;
        var command = new LoginUserCommand(
            "  ivan@example.com  ",
            " Password123 ");

        var result = await service.LoginAsync(command, cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedUser, result.Value);
        Assert.Equal(1, identityService.LoginCallCount);
        Assert.Equal("ivan@example.com", identityService.ReceivedLoginCommand!.Email);
        Assert.Equal(" Password123 ", identityService.ReceivedLoginCommand.Password);
        Assert.Equal(cancellationToken, identityService.ReceivedLoginCancellationToken);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidCredentials_ReturnsUnauthorized()
    {
        var expectedError = new Error(
            "InvalidCredentials",
            "Invalid email or password.",
            ErrorType.Unauthorized);
        var identityService = new RecordingIdentityService(
            Result<UserResult>.Failure(expectedError));
        var service = CreateService(identityService);
        var command = new LoginUserCommand(
            "ivan@example.com",
            "WrongPassword1");

        var result = await service.LoginAsync(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(expectedError, Assert.Single(result.Errors));
        Assert.Equal(1, identityService.LoginCallCount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task LoginAsync_WithMissingEmail_ReturnsValidationWithoutCallingIdentity(
        string? email)
    {
        var identityService = new RecordingIdentityService(
            Result<UserResult>.Success(CreateUserResult()));
        var service = CreateService(identityService);
        var command = new LoginUserCommand(email!, "Password123");

        var result = await service.LoginAsync(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, Assert.Single(result.Errors).Type);
        Assert.Equal(0, identityService.LoginCallCount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task LoginAsync_WithMissingPassword_ReturnsValidationWithoutCallingIdentity(
        string? password)
    {
        var identityService = new RecordingIdentityService(
            Result<UserResult>.Success(CreateUserResult()));
        var service = CreateService(identityService);
        var command = new LoginUserCommand("ivan@example.com", password!);

        var result = await service.LoginAsync(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, Assert.Single(result.Errors).Type);
        Assert.Equal(0, identityService.LoginCallCount);
    }

    [Fact]
    public async Task GetCurrentUserAsync_WithAuthenticatedUser_ReturnsIdentityUser()
    {
        var expectedUser = CreateUserResult();
        var identityService = new RecordingIdentityService(
            Result<UserResult>.Success(expectedUser));
        var currentUser = new StubCurrentUser(true, expectedUser.Id);
        var service = new AuthenticationService(identityService, currentUser);
        var cancellationToken = new CancellationTokenSource().Token;

        var result = await service.GetCurrentUserAsync(cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedUser, result.Value);
        Assert.Equal(1, identityService.GetUserCallCount);
        Assert.Equal(expectedUser.Id, identityService.ReceivedUserId);
        Assert.Equal(cancellationToken, identityService.ReceivedGetUserCancellationToken);
    }

    [Fact]
    public async Task GetCurrentUserAsync_WithUnauthenticatedUser_ReturnsUnauthorizedWithoutIdentityCall()
    {
        var identityService = new RecordingIdentityService(
            Result<UserResult>.Success(CreateUserResult()));
        var currentUser = new StubCurrentUser(false, Guid.Empty);
        var service = new AuthenticationService(identityService, currentUser);

        var result = await service.GetCurrentUserAsync(CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, Assert.Single(result.Errors).Type);
        Assert.Equal(0, identityService.GetUserCallCount);
    }

    [Fact]
    public async Task GetCurrentUserAsync_WhenPersistedUserIsMissing_ReturnsUnauthorized()
    {
        var identityService = new RecordingIdentityService(
            Result<UserResult>.Failure(
                new Error(
                    "MissingUser",
                    "User was not found.",
                    ErrorType.Unauthorized)));
        var currentUser = new StubCurrentUser(true, Guid.NewGuid());
        var service = new AuthenticationService(identityService, currentUser);

        var result = await service.GetCurrentUserAsync(CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, Assert.Single(result.Errors).Type);
        Assert.Equal(1, identityService.GetUserCallCount);
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

    private static AuthenticationService CreateService(
        IIdentityService identityService)
    {
        return new AuthenticationService(
            identityService,
            new StubCurrentUser(false, Guid.Empty));
    }

    private sealed class RecordingIdentityService : IIdentityService
    {
        private readonly Result<UserResult> _result;

        public RecordingIdentityService(Result<UserResult> result)
        {
            _result = result;
        }

        public int CallCount { get; private set; }
        public int GetUserCallCount { get; private set; }
        public int LoginCallCount { get; private set; }
        public Guid ReceivedUserId { get; private set; }
        public RegisterUserCommand? ReceivedCommand { get; private set; }
        public LoginUserCommand? ReceivedLoginCommand { get; private set; }
        public CancellationToken ReceivedCancellationToken { get; private set; }
        public CancellationToken ReceivedGetUserCancellationToken { get; private set; }
        public CancellationToken ReceivedLoginCancellationToken { get; private set; }

        public Task<UserResult?> GetUserAsync(
            Guid userId,
            CancellationToken cancellationToken)
        {
            GetUserCallCount++;
            ReceivedUserId = userId;
            ReceivedGetUserCancellationToken = cancellationToken;

            return Task.FromResult(_result.Value);
        }

        public Task<Result<UserResult>> LoginAsync(
            LoginUserCommand command,
            CancellationToken cancellationToken)
        {
            LoginCallCount++;
            ReceivedLoginCommand = command;
            ReceivedLoginCancellationToken = cancellationToken;

            return Task.FromResult(_result);
        }

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

    private sealed class StubCurrentUser : ICurrentUser
    {
        public StubCurrentUser(bool isAuthenticated, Guid userId)
        {
            IsAuthenticated = isAuthenticated;
            UserId = userId;
        }

        public bool IsAuthenticated { get; }
        public Guid UserId { get; }
    }
}
