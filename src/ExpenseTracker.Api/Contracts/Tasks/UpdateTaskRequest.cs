using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Api.Contracts.Tasks;

public sealed class UpdateTaskRequest
{
    [Required]
    [StringLength(100)]
    public string Title { get; init; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; init; }

    [Required]
    [RegularExpression("^(pending|inProgress|completed)$")]
    public string Status { get; init; } = string.Empty;

    [Required]
    public DateOnly? DueDate { get; init; }
}
