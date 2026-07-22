using ExpenseTracker.Domain.Budgets;
using ExpenseTracker.Domain.Common;
using Xunit;

namespace ExpenseTracker.Domain.UnitTests.Budgets;

public sealed class BudgetTests
{
    [Fact]
    public void Create_WithEmptyName_ThrowsDomainValidationException()
    {
        var createBudget = () => new Budget(
            userId: Guid.NewGuid(),
            name: string.Empty,
            startingBalance: 0m,
            createdAtUtc: new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero));

        Assert.Throws<DomainValidationException>(createBudget);
    }

    [Fact]
    public void Create_WithNullName_ThrowsDomainValidationException()
    {
        var createBudget = () => new Budget(
            userId: Guid.NewGuid(),
            name: null!,
            startingBalance: 0m,
            createdAtUtc: new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero));

        Assert.Throws<DomainValidationException>(createBudget);
    }

    [Fact]
    public void Create_WithWhitespaceOnlyName_ThrowsDomainValidationException()
    {
        var createBudget = () => new Budget(
            userId: Guid.NewGuid(),
            name: "   ",
            startingBalance: 0m,
            createdAtUtc: new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero));

        Assert.Throws<DomainValidationException>(createBudget);
    }

    [Fact]
    public void Create_TrimsName()
    {
        var budget = new Budget(
            userId: Guid.NewGuid(),
            name: "  Monthly expenses  ",
            startingBalance: 0m,
            createdAtUtc: new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero));

        Assert.Equal("Monthly expenses", budget.Name);
    }

    [Fact]
    public void Create_WithExactly100CharacterName_Succeeds()
    {
        var name = new string('a', 100);

        var budget = new Budget(
            userId: Guid.NewGuid(),
            name: name,
            startingBalance: 0m,
            createdAtUtc: new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero));

        Assert.Equal(name, budget.Name);
    }

    [Fact]
    public void Create_WithNameLongerThan100Characters_ThrowsDomainValidationException()
    {
        var createBudget = () => new Budget(
            userId: Guid.NewGuid(),
            name: $" {new string('a', 101)} ",
            startingBalance: 0m,
            createdAtUtc: new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero));

        Assert.Throws<DomainValidationException>(createBudget);
    }

    [Fact]
    public void Create_WithNegativeStartingBalance_ThrowsDomainValidationException()
    {
        var createBudget = () => new Budget(
            userId: Guid.NewGuid(),
            name: "Monthly expenses",
            startingBalance: -0.01m,
            createdAtUtc: new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero));

        Assert.Throws<DomainValidationException>(createBudget);
    }

    [Fact]
    public void Create_WithZeroStartingBalance_Succeeds()
    {
        var budget = new Budget(
            userId: Guid.NewGuid(),
            name: "Monthly expenses",
            startingBalance: 0m,
            createdAtUtc: new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero));

        Assert.Equal(0m, budget.StartingBalance);
    }

    [Fact]
    public void Create_WithTwoDecimalStartingBalance_Succeeds()
    {
        var budget = new Budget(
            userId: Guid.NewGuid(),
            name: "Monthly expenses",
            startingBalance: 1000.25m,
            createdAtUtc: new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero));

        Assert.Equal(1000.25m, budget.StartingBalance);
    }

    [Fact]
    public void Create_WithStartingBalanceHavingScaleGreaterThanTwo_ThrowsDomainValidationException()
    {
        var createBudget = () => new Budget(
            userId: Guid.NewGuid(),
            name: "Monthly expenses",
            startingBalance: 1.230m,
            createdAtUtc: new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero));

        Assert.Throws<DomainValidationException>(createBudget);
    }

    [Fact]
    public void Create_WithValidArguments_PreservesBudgetData()
    {
        var userId = Guid.NewGuid();
        var createdAtUtc = new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero);

        var budget = new Budget(
            userId: userId,
            name: "Monthly expenses",
            startingBalance: 1000.25m,
            createdAtUtc: createdAtUtc);

        Assert.Equal(0, budget.Id);
        Assert.Equal(userId, budget.UserId);
        Assert.Equal("Monthly expenses", budget.Name);
        Assert.Equal(1000.25m, budget.StartingBalance);
        Assert.Equal(createdAtUtc, budget.CreatedAtUtc);
    }

    [Fact]
    public void Rename_WithValidName_TrimsAndUpdatesName()
    {
        var budget = new Budget(
            userId: Guid.NewGuid(),
            name: "Monthly expenses",
            startingBalance: 0m,
            createdAtUtc: new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero));

        budget.Rename("  Household expenses  ");

        Assert.Equal("Household expenses", budget.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rename_WithInvalidName_ThrowsAndPreservesExistingName(string? name)
    {
        var budget = new Budget(
            userId: Guid.NewGuid(),
            name: "Monthly expenses",
            startingBalance: 0m,
            createdAtUtc: new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero));

        var renameBudget = () => budget.Rename(name!);

        Assert.Throws<DomainValidationException>(renameBudget);
        Assert.Equal("Monthly expenses", budget.Name);
    }

    [Fact]
    public void Rename_WithNameLongerThan100Characters_ThrowsAndPreservesExistingName()
    {
        var budget = new Budget(
            userId: Guid.NewGuid(),
            name: "Monthly expenses",
            startingBalance: 0m,
            createdAtUtc: new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero));

        var renameBudget = () => budget.Rename($" {new string('a', 101)} ");

        Assert.Throws<DomainValidationException>(renameBudget);
        Assert.Equal("Monthly expenses", budget.Name);
    }

    [Fact]
    public void UpdateStartingBalance_WithValidValue_UpdatesStartingBalance()
    {
        var budget = new Budget(
            userId: Guid.NewGuid(),
            name: "Monthly expenses",
            startingBalance: 1000m,
            createdAtUtc: new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero));

        budget.UpdateStartingBalance(1250.25m);

        Assert.Equal(1250.25m, budget.StartingBalance);
    }

    [Fact]
    public void UpdateStartingBalance_WithZero_UpdatesStartingBalance()
    {
        var budget = new Budget(
            userId: Guid.NewGuid(),
            name: "Monthly expenses",
            startingBalance: 1000m,
            createdAtUtc: new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero));

        budget.UpdateStartingBalance(0m);

        Assert.Equal(0m, budget.StartingBalance);
    }

    [Fact]
    public void UpdateStartingBalance_WithNegativeValue_ThrowsAndPreservesExistingBalance()
    {
        var budget = new Budget(
            userId: Guid.NewGuid(),
            name: "Monthly expenses",
            startingBalance: 1000m,
            createdAtUtc: new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero));

        var updateStartingBalance = () => budget.UpdateStartingBalance(-0.01m);

        Assert.Throws<DomainValidationException>(updateStartingBalance);
        Assert.Equal(1000m, budget.StartingBalance);
    }

    [Fact]
    public void UpdateStartingBalance_WithScaleGreaterThanTwo_ThrowsAndPreservesExistingBalance()
    {
        var budget = new Budget(
            userId: Guid.NewGuid(),
            name: "Monthly expenses",
            startingBalance: 1000m,
            createdAtUtc: new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero));

        var updateStartingBalance = () => budget.UpdateStartingBalance(1.230m);

        Assert.Throws<DomainValidationException>(updateStartingBalance);
        Assert.Equal(1000m, budget.StartingBalance);
    }
}
