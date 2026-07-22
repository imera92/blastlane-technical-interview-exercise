using ExpenseTracker.Domain.Common;

namespace ExpenseTracker.Domain.Budgets;

public class Budget
{
    public long Id { get; private set; }
    public Guid UserId { get; }
    public string Name { get; private set; }
    public decimal StartingBalance { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; }

    public Budget(
        Guid userId,
        string name,
        decimal startingBalance,
        DateTimeOffset createdAtUtc)
    {
        UserId = userId;
        Name = ValidateName(name);
        StartingBalance = ValidateStartingBalance(startingBalance);
        CreatedAtUtc = createdAtUtc;
    }

    public void Rename(string name)
    {
        Name = ValidateName(name);
    }

    public void UpdateStartingBalance(decimal startingBalance)
    {
        StartingBalance = ValidateStartingBalance(startingBalance);
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainValidationException("Budget name is required.");
        }

        var trimmedName = name.Trim();

        if (trimmedName.Length > 100)
        {
            throw new DomainValidationException("Budget name cannot exceed 100 characters.");
        }

        return trimmedName;
    }

    private static decimal ValidateStartingBalance(decimal startingBalance)
    {
        if (startingBalance < 0m)
        {
            throw new DomainValidationException("Budget starting balance cannot be negative.");
        }

        if (!MoneyPrecision.HasAtMostTwoDecimalPlaces(startingBalance))
        {
            throw new DomainValidationException(
                "Budget starting balance cannot exceed two decimal places.");
        }

        return startingBalance;
    }
}
