using ExpenseTracker.Domain.Budgets;
using ExpenseTracker.Domain.Common;
using Xunit;

namespace ExpenseTracker.Domain.UnitTests.Transactions;

public sealed class BudgetTransactionTests
{
    [Fact]
    public void Create_WithValidArguments_PreservesTransactionData()
    {
        var date = new DateOnly(2026, 7, 20);
        var createdAtUtc = new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero);
        var budget = CreateBudget();

        var transaction = budget.AddTransaction(
            name: "Groceries",
            amount: -74.50m,
            date: date,
            createdAtUtc: createdAtUtc);

        Assert.Equal(0, transaction.Id);
        Assert.Equal(budget.Id, transaction.BudgetId);
        Assert.Equal("Groceries", transaction.Name);
        Assert.Equal(-74.50m, transaction.Amount);
        Assert.Equal(date, transaction.Date);
        Assert.Equal(createdAtUtc, transaction.CreatedAtUtc);
    }

    [Fact]
    public void Create_WithPositiveAmountHavingScaleGreaterThanTwo_ThrowsDomainValidationException()
    {
        var createTransaction = () => CreateBudget().AddTransaction(
            name: "Income",
            amount: 1.230m,
            date: new DateOnly(2026, 7, 21),
            createdAtUtc: new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero));

        Assert.Throws<DomainValidationException>(createTransaction);
    }

    [Fact]
    public void Create_WithNegativeAmountHavingScaleGreaterThanTwo_ThrowsDomainValidationException()
    {
        var createTransaction = () => CreateBudget().AddTransaction(
            name: "Expense",
            amount: -1.230m,
            date: new DateOnly(2026, 7, 21),
            createdAtUtc: new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero));

        Assert.Throws<DomainValidationException>(createTransaction);
    }

    [Fact]
    public void Create_WithTwoDecimalAmount_Succeeds()
    {
        var transaction = CreateBudget().AddTransaction(
            name: "Expense",
            amount: -1.23m,
            date: new DateOnly(2026, 7, 21),
            createdAtUtc: new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero));

        Assert.Equal(-1.23m, transaction.Amount);
    }

    [Fact]
    public void Create_TrimsName()
    {
        var transaction = CreateBudget().AddTransaction(
            name: "  Groceries  ",
            amount: -74.50m,
            date: new DateOnly(2026, 7, 21),
            createdAtUtc: new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero));

        Assert.Equal("Groceries", transaction.Name);
    }

    [Fact]
    public void Create_WithNegativeAmount_PreservesSignedAmount()
    {
        var transaction = CreateBudget().AddTransaction(
            name: "Groceries",
            amount: -74.50m,
            date: new DateOnly(2026, 7, 21),
            createdAtUtc: new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero));

        Assert.Equal(-74.50m, transaction.Amount);
    }

    [Fact]
    public void Create_WithPositiveAmount_PreservesSignedAmount()
    {
        var transaction = CreateBudget().AddTransaction(
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

        var transaction = CreateBudget().AddTransaction(
            name: name,
            amount: -10m,
            date: new DateOnly(2026, 7, 21),
            createdAtUtc: new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero));

        Assert.Equal(name, transaction.Name);
    }

    [Fact]
    public void Create_WithNameLongerThan100Characters_ThrowsDomainValidationException()
    {
        var createTransaction = () => CreateBudget().AddTransaction(
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
        var createTransaction = () => CreateBudget().AddTransaction(
            name: name!,
            amount: -10m,
            date: new DateOnly(2026, 7, 21),
            createdAtUtc: new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero));

        Assert.Throws<DomainValidationException>(createTransaction);
    }

    [Fact]
    public void Create_WithZeroAmount_ThrowsDomainValidationException()
    {
        var createTransaction = () => CreateBudget().AddTransaction(
            name: "Groceries",
            amount: 0m,
            date: new DateOnly(2026, 7, 21),
            createdAtUtc: new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero));

        Assert.Throws<DomainValidationException>(createTransaction);
    }

    [Fact]
    public void Update_WithValidValues_UpdatesTransactionData()
    {
        var transaction = CreateBudget().AddTransaction(
            "Groceries",
            -10m,
            new DateOnly(2026, 7, 21),
            new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero));

        transaction.Update("  Salary  ", 250m, new DateOnly(2026, 7, 22));

        Assert.Equal("Salary", transaction.Name);
        Assert.Equal(250m, transaction.Amount);
        Assert.Equal(new DateOnly(2026, 7, 22), transaction.Date);
    }

    [Fact]
    public void Update_WithInvalidAmount_PreservesAllTransactionData()
    {
        var createdAt = new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero);
        var transaction = CreateBudget().AddTransaction("Groceries", -10m, new DateOnly(2026, 7, 21), createdAt);

        var update = () => transaction.Update("Salary", 0m, new DateOnly(2026, 7, 22));

        Assert.Throws<DomainValidationException>(update);
        Assert.Equal("Groceries", transaction.Name);
        Assert.Equal(-10m, transaction.Amount);
        Assert.Equal(new DateOnly(2026, 7, 21), transaction.Date);
        Assert.Equal(createdAt, transaction.CreatedAtUtc);
    }

    private static Budget CreateBudget()
    {
        return new Budget(
            userId: Guid.NewGuid(),
            name: "Monthly expenses",
            startingBalance: 0m,
            createdAtUtc: new DateTimeOffset(2026, 7, 21, 11, 0, 0, TimeSpan.Zero));
    }
}
