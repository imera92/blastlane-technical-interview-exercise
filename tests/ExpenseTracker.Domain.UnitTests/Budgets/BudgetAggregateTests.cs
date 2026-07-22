using ExpenseTracker.Domain.Budgets;
using ExpenseTracker.Domain.Common;
using Xunit;

namespace ExpenseTracker.Domain.UnitTests.Budgets;

public sealed class BudgetAggregateTests
{
    [Fact]
    public void Create_HasNoTransactions()
    {
        var budget = new Budget(
            userId: Guid.NewGuid(),
            name: "Monthly expenses",
            startingBalance: 1000m,
            createdAtUtc: new DateTimeOffset(2026, 7, 22, 12, 0, 0, TimeSpan.Zero));

        Assert.Empty(budget.Transactions);
    }

    [Fact]
    public void Create_CurrentBalanceEqualsStartingBalance()
    {
        var budget = new Budget(
            userId: Guid.NewGuid(),
            name: "Monthly expenses",
            startingBalance: 1000.25m,
            createdAtUtc: new DateTimeOffset(2026, 7, 22, 12, 0, 0, TimeSpan.Zero));

        Assert.Equal(1000.25m, budget.CurrentBalance);
    }

    [Fact]
    public void AddTransaction_WithValidArguments_AddsAndReturnsTransactionBelongingToBudget()
    {
        var budget = new Budget(
            userId: Guid.NewGuid(),
            name: "Monthly expenses",
            startingBalance: 1000m,
            createdAtUtc: new DateTimeOffset(2026, 7, 22, 12, 0, 0, TimeSpan.Zero));

        var transaction = budget.AddTransaction(
            name: "Groceries",
            amount: -74.50m,
            date: new DateOnly(2026, 7, 22),
            createdAtUtc: new DateTimeOffset(2026, 7, 22, 12, 30, 0, TimeSpan.Zero));

        Assert.Same(transaction, Assert.Single(budget.Transactions));
        Assert.Equal(budget.Id, transaction.BudgetId);
    }

    [Fact]
    public void AddTransaction_WithIncome_IncreasesCurrentBalance()
    {
        var budget = new Budget(
            userId: Guid.NewGuid(),
            name: "Monthly expenses",
            startingBalance: 1000m,
            createdAtUtc: new DateTimeOffset(2026, 7, 22, 12, 0, 0, TimeSpan.Zero));

        budget.AddTransaction(
            name: "Salary",
            amount: 250.25m,
            date: new DateOnly(2026, 7, 22),
            createdAtUtc: new DateTimeOffset(2026, 7, 22, 12, 30, 0, TimeSpan.Zero));

        Assert.Equal(1250.25m, budget.CurrentBalance);
    }

    [Fact]
    public void AddTransaction_WithExpense_DecreasesCurrentBalance()
    {
        var budget = new Budget(
            userId: Guid.NewGuid(),
            name: "Monthly expenses",
            startingBalance: 1000m,
            createdAtUtc: new DateTimeOffset(2026, 7, 22, 12, 0, 0, TimeSpan.Zero));

        budget.AddTransaction(
            name: "Groceries",
            amount: -74.50m,
            date: new DateOnly(2026, 7, 22),
            createdAtUtc: new DateTimeOffset(2026, 7, 22, 12, 30, 0, TimeSpan.Zero));

        Assert.Equal(925.50m, budget.CurrentBalance);
    }

    [Fact]
    public void CurrentBalance_WithMultipleTransactions_ReturnsCorrectBalance()
    {
        var budget = new Budget(
            userId: Guid.NewGuid(),
            name: "Monthly expenses",
            startingBalance: 1000m,
            createdAtUtc: new DateTimeOffset(2026, 7, 22, 12, 0, 0, TimeSpan.Zero));

        budget.AddTransaction(
            name: "Utilities",
            amount: -100m,
            date: new DateOnly(2026, 7, 22),
            createdAtUtc: new DateTimeOffset(2026, 7, 22, 12, 30, 0, TimeSpan.Zero));
        budget.AddTransaction(
            name: "Salary",
            amount: 250m,
            date: new DateOnly(2026, 7, 22),
            createdAtUtc: new DateTimeOffset(2026, 7, 22, 13, 0, 0, TimeSpan.Zero));
        budget.AddTransaction(
            name: "Groceries",
            amount: -74.50m,
            date: new DateOnly(2026, 7, 22),
            createdAtUtc: new DateTimeOffset(2026, 7, 22, 13, 30, 0, TimeSpan.Zero));

        Assert.Equal(1075.50m, budget.CurrentBalance);
    }

    [Fact]
    public void CurrentBalance_WhenExpensesExceedAvailableFunds_IsNegative()
    {
        var budget = new Budget(
            userId: Guid.NewGuid(),
            name: "Monthly expenses",
            startingBalance: 50m,
            createdAtUtc: new DateTimeOffset(2026, 7, 22, 12, 0, 0, TimeSpan.Zero));

        budget.AddTransaction(
            name: "Groceries",
            amount: -75m,
            date: new DateOnly(2026, 7, 22),
            createdAtUtc: new DateTimeOffset(2026, 7, 22, 12, 30, 0, TimeSpan.Zero));

        Assert.Equal(-25m, budget.CurrentBalance);
    }

    [Fact]
    public void AddTransaction_WithInvalidArguments_ThrowsAndLeavesCollectionUnchanged()
    {
        var budget = new Budget(
            userId: Guid.NewGuid(),
            name: "Monthly expenses",
            startingBalance: 1000m,
            createdAtUtc: new DateTimeOffset(2026, 7, 22, 12, 0, 0, TimeSpan.Zero));

        var addTransaction = () => budget.AddTransaction(
            name: "Invalid transaction",
            amount: 0m,
            date: new DateOnly(2026, 7, 22),
            createdAtUtc: new DateTimeOffset(2026, 7, 22, 12, 30, 0, TimeSpan.Zero));

        Assert.Throws<DomainValidationException>(addTransaction);
        Assert.Empty(budget.Transactions);
        Assert.Equal(1000m, budget.CurrentBalance);
    }

    [Fact]
    public void UpdateStartingBalance_RecalculatesCurrentBalanceWithoutChangingTransactions()
    {
        var budget = new Budget(
            userId: Guid.NewGuid(),
            name: "Monthly expenses",
            startingBalance: 1000m,
            createdAtUtc: new DateTimeOffset(2026, 7, 22, 12, 0, 0, TimeSpan.Zero));
        var transaction = budget.AddTransaction(
            name: "Utilities",
            amount: -100m,
            date: new DateOnly(2026, 7, 22),
            createdAtUtc: new DateTimeOffset(2026, 7, 22, 12, 30, 0, TimeSpan.Zero));

        budget.UpdateStartingBalance(1250m);

        Assert.Equal(1150m, budget.CurrentBalance);
        Assert.Same(transaction, Assert.Single(budget.Transactions));
    }
}
