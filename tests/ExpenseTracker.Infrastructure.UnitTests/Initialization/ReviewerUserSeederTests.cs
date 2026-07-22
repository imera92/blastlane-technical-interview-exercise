using ExpenseTracker.Infrastructure.Authentication;
using ExpenseTracker.Infrastructure.Initialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace ExpenseTracker.Infrastructure.UnitTests.Initialization;

public sealed class ReviewerUserSeederTests
{
    [Fact]
    public async Task SeedAsync_WhenEnabledAndUserDoesNotExist_CreatesConfiguredUser()
    {
        var createdAtUtc = new DateTimeOffset(2026, 7, 22, 15, 30, 0, TimeSpan.Zero);
        var userManager = new RecordingUserManager();
        var seeder = new ReviewerUserSeeder(
            userManager,
            new FixedTimeProvider(createdAtUtc));
        var options = new ReviewerUserOptions
        {
            Enabled = true,
            DisplayName = "Reviewer",
            Email = "reviewer@example.com",
            Password = "Reviewer123"
        };

        await seeder.SeedAsync(options, CancellationToken.None);

        Assert.Equal("reviewer@example.com", userManager.ReceivedLookupEmail);
        Assert.Equal(1, userManager.CreateCallCount);
        Assert.Equal("Reviewer123", userManager.ReceivedPassword);
        Assert.Equal("Reviewer", userManager.ReceivedUser!.DisplayName);
        Assert.Equal("reviewer@example.com", userManager.ReceivedUser.Email);
        Assert.Equal("reviewer@example.com", userManager.ReceivedUser.UserName);
        Assert.Equal(createdAtUtc, userManager.ReceivedUser.CreatedAtUtc);
    }

    [Fact]
    public async Task SeedAsync_WhenConfiguredUserAlreadyExists_DoesNotCreateAnotherUser()
    {
        var existingUser = new ApplicationUser
        {
            Email = "reviewer@example.com"
        };
        var userManager = new RecordingUserManager(existingUser);
        var seeder = new ReviewerUserSeeder(userManager, TimeProvider.System);

        await seeder.SeedAsync(CreateOptions(), CancellationToken.None);

        Assert.Equal("reviewer@example.com", userManager.ReceivedLookupEmail);
        Assert.Equal(0, userManager.CreateCallCount);
    }

    [Fact]
    public async Task SeedAsync_WhenDisabled_DoesNotQueryOrCreateUser()
    {
        var userManager = new RecordingUserManager();
        var seeder = new ReviewerUserSeeder(userManager, TimeProvider.System);
        var options = CreateOptions();
        options.Enabled = false;

        await seeder.SeedAsync(options, CancellationToken.None);

        Assert.Null(userManager.ReceivedLookupEmail);
        Assert.Equal(0, userManager.CreateCallCount);
    }

    [Fact]
    public async Task SeedAsync_WhenIdentityRejectsUser_ThrowsWithoutExposingPassword()
    {
        var identityError = new IdentityError
        {
            Code = "SeedRejected",
            Description = "The configured reviewer user is invalid."
        };
        var userManager = new RecordingUserManager(
            createResult: IdentityResult.Failed(identityError));
        var seeder = new ReviewerUserSeeder(userManager, TimeProvider.System);
        var options = CreateOptions();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => seeder.SeedAsync(options, CancellationToken.None));

        Assert.Contains(identityError.Description, exception.Message);
        Assert.DoesNotContain(options.Password, exception.Message);
    }

    private static ReviewerUserOptions CreateOptions()
    {
        return new ReviewerUserOptions
        {
            Enabled = true,
            DisplayName = "Reviewer",
            Email = "reviewer@example.com",
            Password = "Reviewer123"
        };
    }

    private sealed class RecordingUserManager : UserManager<ApplicationUser>
    {
        private readonly ApplicationUser? _existingUser;
        private readonly IdentityResult _createResult;

        public RecordingUserManager(
            ApplicationUser? existingUser = null,
            IdentityResult? createResult = null)
            : base(
                new StubUserStore(),
                Microsoft.Extensions.Options.Options.Create(new IdentityOptions()),
                new PasswordHasher<ApplicationUser>(),
                [],
                [],
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                null!,
                NullLogger<UserManager<ApplicationUser>>.Instance)
        {
            _existingUser = existingUser;
            _createResult = createResult ?? IdentityResult.Success;
        }

        public string? ReceivedLookupEmail { get; private set; }
        public ApplicationUser? ReceivedUser { get; private set; }
        public string? ReceivedPassword { get; private set; }
        public int CreateCallCount { get; private set; }

        public override Task<ApplicationUser?> FindByEmailAsync(string email)
        {
            ReceivedLookupEmail = email;
            return Task.FromResult(_existingUser);
        }

        public override Task<IdentityResult> CreateAsync(
            ApplicationUser user,
            string password)
        {
            CreateCallCount++;
            ReceivedUser = user;
            ReceivedPassword = password;
            return Task.FromResult(_createResult);
        }
    }

    private sealed class StubUserStore : IUserStore<ApplicationUser>
    {
        public void Dispose()
        {
        }

        public Task<string> GetUserIdAsync(
            ApplicationUser user,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<string?> GetUserNameAsync(
            ApplicationUser user,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task SetUserNameAsync(
            ApplicationUser user,
            string? userName,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<string?> GetNormalizedUserNameAsync(
            ApplicationUser user,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task SetNormalizedUserNameAsync(
            ApplicationUser user,
            string? normalizedName,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IdentityResult> CreateAsync(
            ApplicationUser user,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IdentityResult> UpdateAsync(
            ApplicationUser user,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IdentityResult> DeleteAsync(
            ApplicationUser user,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<ApplicationUser?> FindByIdAsync(
            string userId,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<ApplicationUser?> FindByNameAsync(
            string normalizedUserName,
            CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public FixedTimeProvider(DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }

        public override DateTimeOffset GetUtcNow() => _utcNow;
    }
}
