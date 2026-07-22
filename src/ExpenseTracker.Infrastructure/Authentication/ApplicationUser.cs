using Microsoft.AspNetCore.Identity;

namespace ExpenseTracker.Infrastructure.Authentication;

public class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
}
