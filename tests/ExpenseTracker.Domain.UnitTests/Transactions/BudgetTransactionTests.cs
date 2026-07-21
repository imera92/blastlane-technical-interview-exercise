using ExpenseTracker.Domain.Common;
using ExpenseTracker.Domain.Transactions;
using Xunit;

namespace ExpenseTracker.Domain.UnitTests.Transactions;

public sealed class BudgetTransactionTests
{
    [Fact]
    public void Create_WithValidArguments_PreservesTransactionData()
    {
        var date = new DateOnly(2026, 7, 20);
        var createdAtUtc = new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero);

        var transaction = new BudgetTransaction(
            budgetId: 42,
            name: "Groceries",
            amount: -74.50m,
            date: date,
            createdAtUtc: createdAtUtc);

        Assert.Equal(0, transaction.Id);
        Assert.Equal(42, transaction.BudgetId);
        Assert.Equal("Groceries", transaction.Name);
        Assert.Equal(-74.50m, transaction.Amount);
        Assert.Equal(date, transaction.Date);
        Assert.Equal(createdAtUtc, transaction.CreatedAtUtc);
    }

    [Fact]
    public void Create_WithPositiveAmountHavingScaleGreaterThanTwo_ThrowsDomainValidationException()
    {
        var createTransaction = () => new BudgetTransaction(
            budgetId: 1,
            name: "Income",
            amount: 1.230m,
            date: new DateOnly(2026, 7, 21),
            createdAtUtc: new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero));

        Assert.Throws<DomainValidationException>(createTransaction);
    }

    [Fact]
    public void Create_WithNegativeAmountHavingScaleGreaterThanTwo_ThrowsDomainValidationException()
    {
        var createTransaction = () => new BudgetTransaction(
            budgetId: 1,
            name: "Expense",
            amount: -1.230m,
            date: new DateOnly(2026, 7, 21),
            createdAtUtc: new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero));

        Assert.Throws<DomainValidationException>(createTransaction);
    }

    [Fact]
    public void Create_WithTwoDecimalAmount_Succeeds()
    {
        var transaction = new BudgetTransaction(
            budgetId: 1,
            name: "Expense",
            amount: -1.23m,
            date: new DateOnly(2026, 7, 21),
            createdAtUtc: new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero));

        Assert.Equal(-1.23m, transaction.Amount);
    }

    [Fact]
    public void Create_TrimsName()
    {
        var transaction = new BudgetTransaction(
            budgetId: 1,
            name: "  Groceries  ",
            amount: -74.50m,
            date: new DateOnly(2026, 7, 21),
            createdAtUtc: new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero));

        Assert.Equal("Groceries", transaction.Name);
    }

    [Fact]
    public void Create_WithNegativeAmount_PreservesSignedAmount()
    {
        var transaction = new BudgetTransaction(
            budgetId: 1,
            name: "Groceries",
            amount: -74.50m,
            date: new DateOnly(2026, 7, 21),
            createdAtUtc: new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero));

        Assert.Equal(-74.50m, transaction.Amount);
    }

    [Fact]
    public void Create_WithPositiveAmount_PreservesSignedAmount()
    {
        var transaction = new BudgetTransaction(
            budgetId: 1,
            name: "Salary",
            amount: 250.25m,
            date: new DateOnly(2026, 7, 21),
            createdAtUtc: new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero));

        Assert.Equal(250.25m, transaction.Amount);
    }

    [Fact]
    public void Create_WithExactly100CharacterName_Succeeds()
    {
        var name = new string('a', 100);

        var transaction = new BudgetTransaction(
            budgetId: 1,
            name: name,
            amount: -10m,
            date: new DateOnly(2026, 7, 21),
            createdAtUtc: new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero));

        Assert.Equal(name, transaction.Name);
    }

    [Fact]
    public void Create_WithNameLongerThan100Characters_ThrowsDomainValidationException()
    {
        var createTransaction = () => new BudgetTransaction(
            budgetId: 1,
            name: $" {new string('a', 101)} ",
            amount: -10m,
            date: new DateOnly(2026, 7, 21),
            createdAtUtc: new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero));

        Assert.Throws<DomainValidationException>(createTransaction);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ThrowsDomainValidationException(string? name)
    {
        var createTransaction = () => new BudgetTransaction(
            budgetId: 1,
            name: name!,
            amount: -10m,
            date: new DateOnly(2026, 7, 21),
            createdAtUtc: new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero));

        Assert.Throws<DomainValidationException>(createTransaction);
    }

    [Fact]
    public void Create_WithZeroAmount_ThrowsDomainValidationException()
    {
        var createTransaction = () => new BudgetTransaction(
            budgetId: 1,
            name: "Groceries",
            amount: 0m,
            date: new DateOnly(2026, 7, 21),
            createdAtUtc: new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero));

        Assert.Throws<DomainValidationException>(createTransaction);
    }
}
