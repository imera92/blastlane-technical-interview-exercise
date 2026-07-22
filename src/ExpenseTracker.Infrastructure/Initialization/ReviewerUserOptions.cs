using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Infrastructure.Initialization;

public sealed class ReviewerUserOptions
{
    public const string SectionName = "ReviewerUser";

    public bool Enabled { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string DisplayName { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}
