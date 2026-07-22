using ExpenseTracker.Domain.Common;

namespace ExpenseTracker.Domain.Transactions;

public class BudgetTransaction
{
    private long _budgetId;

    public long Id { get; private set; }
    public long BudgetId => _budgetId;
    public string Name { get; private set; }
    public decimal Amount { get; private set; }
    public DateOnly Date { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; }

    internal BudgetTransaction(
        long budgetId,
        string name,
        decimal amount,
        DateOnly date,
        DateTimeOffset createdAtUtc)
    {
        _budgetId = budgetId;
        Name = ValidateName(name);
        Amount = ValidateAmount(amount);
        Date = date;
        CreatedAtUtc = createdAtUtc;
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainValidationException("Transaction name is required.");
        }

        var trimmedName = name.Trim();

        if (trimmedName.Length > 100)
        {
            throw new DomainValidationException("Transaction name cannot exceed 100 characters.");
        }

        return trimmedName;
    }

    private static decimal ValidateAmount(decimal amount)
    {
        if (amount == 0m)
        {
            throw new DomainValidationException("Transaction amount cannot be zero.");
        }

        if (!MoneyPrecision.HasAtMostTwoDecimalPlaces(amount))
        {
            throw new DomainValidationException("Transaction amount cannot exceed two decimal places.");
        }

        return amount;
    }
}
