using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Api.Contracts.Budgets;

public sealed class UpdateBudgetRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; init; } = string.Empty;
    public decimal StartingBalance { get; init; }
}
