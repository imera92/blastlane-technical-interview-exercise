using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Api.Contracts.Authentication;

public sealed class RegisterRequest
{
    [Required]
    [StringLength(100)]
    public string DisplayName { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}
