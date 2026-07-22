using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Api.Contracts.Transactions;

public sealed class TransactionRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateOnly Date { get; init; }
}
