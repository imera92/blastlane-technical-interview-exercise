using ExpenseTracker.Infrastructure.Authentication;
using Microsoft.AspNetCore.Identity;

namespace ExpenseTracker.Infrastructure.Initialization;

public sealed class ReviewerUserSeeder
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly TimeProvider _timeProvider;

    public ReviewerUserSeeder(
        UserManager<ApplicationUser> userManager,
        TimeProvider timeProvider)
    {
        _userManager = userManager;
        _timeProvider = timeProvider;
    }

    public async Task SeedAsync(
        ReviewerUserOptions options,
        CancellationToken cancellationToken)
    {
        if (!options.Enabled)
        {
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();

        var email = options.Email.Trim();
        var existingUser = await _userManager.FindByEmailAsync(email);

        if (existingUser is not null)
        {
            return;
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            DisplayName = options.DisplayName.Trim(),
            Email = email,
            UserName = email,
            CreatedAtUtc = _timeProvider.GetUtcNow()
        };

        var result = await _userManager.CreateAsync(user, options.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(
                " ",
                result.Errors.Select(error => error.Description));

            throw new InvalidOperationException(
                $"The reviewer user could not be created. {errors}");
        }
    }
}
