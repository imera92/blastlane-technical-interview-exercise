using ExpenseTracker.Domain.Budgets;
using ExpenseTracker.Domain.Transactions;
using ExpenseTracker.Infrastructure.Authentication;
using ExpenseTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace ExpenseTracker.Infrastructure.UnitTests.Persistence;

public sealed class ExpenseTrackerDbContextModelTests
{
    [Fact]
    public void BudgetStartingBalance_UsesIntegerCentConversion()
    {
        using var context = CreateContext();

        var budgetType = context.Model.FindEntityType(typeof(Budget));
        var startingBalance = budgetType!.FindProperty(nameof(Budget.StartingBalance));
        var converter = startingBalance!.GetTypeMapping().Converter;

        Assert.NotNull(converter);
        Assert.Equal(typeof(decimal), converter.ModelClrType);
        Assert.Equal(typeof(long), converter.ProviderClrType);
        Assert.Equal("INTEGER", startingBalance.GetColumnType());
    }

    [Fact]
    public void BudgetTransactionAmount_UsesIntegerCentConversion()
    {
        using var context = CreateContext();

        var transactionType = context.Model.FindEntityType(typeof(BudgetTransaction));
        var amount = transactionType!.FindProperty(nameof(BudgetTransaction.Amount));
        var converter = amount!.GetTypeMapping().Converter;

        Assert.NotNull(converter);
        Assert.Equal(typeof(decimal), converter.ModelClrType);
        Assert.Equal(typeof(long), converter.ProviderClrType);
        Assert.Equal("INTEGER", amount.GetColumnType());
    }

    [Fact]
    public void BudgetCurrentBalance_IsNotMapped()
    {
        using var context = CreateContext();

        var budgetType = context.Model.FindEntityType(typeof(Budget));

        Assert.Null(budgetType!.FindProperty(nameof(Budget.CurrentBalance)));
    }

    [Fact]
    public void BudgetTransactions_UsePrivateCollectionAndCascadeDelete()
    {
        using var context = CreateContext();

        var budgetType = context.Model.FindEntityType(typeof(Budget));
        var navigation = budgetType!.FindNavigation(nameof(Budget.Transactions));
        var foreignKey = navigation!.ForeignKey;
        var budgetId = foreignKey.DeclaringEntityType
            .FindProperty(nameof(BudgetTransaction.BudgetId));

        Assert.Equal("_transactions", navigation.FieldInfo!.Name);
        Assert.Equal(PropertyAccessMode.Field, navigation.GetPropertyAccessMode());
        Assert.True(foreignKey.IsRequired);
        Assert.Equal(DeleteBehavior.Cascade, foreignKey.DeleteBehavior);
        Assert.Equal("_budgetId", budgetId!.FieldInfo!.Name);
        Assert.Equal(PropertyAccessMode.Field, budgetId.GetPropertyAccessMode());
    }

    [Fact]
    public void BudgetOwnership_UsesRequiredNonCascadingUserRelationship()
    {
        using var context = CreateContext();

        var budgetType = context.Model.FindEntityType(typeof(Budget));
        var ownership = Assert.Single(
            budgetType!.GetForeignKeys(),
            foreignKey => foreignKey.PrincipalEntityType.ClrType == typeof(ApplicationUser));

        Assert.True(ownership.IsRequired);
        Assert.Equal(DeleteBehavior.Restrict, ownership.DeleteBehavior);
        Assert.Equal(
            nameof(Budget.UserId),
            Assert.Single(ownership.Properties).Name);
    }

    [Fact]
    public void BudgetAndTransactionIndexes_UseRequiredPropertyOrder()
    {
        using var context = CreateContext();

        var budgetType = context.Model.FindEntityType(typeof(Budget));
        var transactionType = context.Model.FindEntityType(typeof(BudgetTransaction));

        Assert.Contains(
            budgetType!.GetIndexes(),
            index => HasProperties(
                index,
                nameof(Budget.UserId),
                nameof(Budget.CreatedAtUtc),
                nameof(Budget.Id)));
        Assert.Contains(
            transactionType!.GetIndexes(),
            index => HasProperties(
                index,
                nameof(BudgetTransaction.BudgetId),
                nameof(BudgetTransaction.Date),
                nameof(BudgetTransaction.CreatedAtUtc),
                nameof(BudgetTransaction.Id)));
    }

    [Fact]
    public void ApplicationUserFields_UseRequiredPersistencePolicies()
    {
        using var context = CreateContext();

        var userType = context.Model.FindEntityType(typeof(ApplicationUser));
        var displayName = userType!.FindProperty(nameof(ApplicationUser.DisplayName));
        var createdAtUtc = userType.FindProperty(nameof(ApplicationUser.CreatedAtUtc));
        var timestampConverter = createdAtUtc!.GetTypeMapping().Converter;
        var emailIndex = Assert.Single(
            userType.GetIndexes(),
            index => HasProperties(index, nameof(ApplicationUser.NormalizedEmail)));

        Assert.False(displayName!.IsNullable);
        Assert.Equal(100, displayName.GetMaxLength());
        Assert.NotNull(timestampConverter);
        Assert.Equal(typeof(long), timestampConverter.ProviderClrType);
        Assert.Equal("INTEGER", createdAtUtc.GetColumnType());
        Assert.True(emailIndex.IsUnique);
    }

    [Fact]
    public void BudgetAndTransactionScalars_UseRequiredPersistencePolicies()
    {
        using var context = CreateContext();

        var budgetType = context.Model.FindEntityType(typeof(Budget));
        var transactionType = context.Model.FindEntityType(typeof(BudgetTransaction));

        AssertScalarPolicies(
            budgetType!,
            nameof(Budget.Id),
            nameof(Budget.Name),
            nameof(Budget.CreatedAtUtc));
        AssertScalarPolicies(
            transactionType!,
            nameof(BudgetTransaction.Id),
            nameof(BudgetTransaction.Name),
            nameof(BudgetTransaction.CreatedAtUtc));
    }

    private static ExpenseTrackerDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ExpenseTrackerDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        return new ExpenseTrackerDbContext(options);
    }

    private static bool HasProperties(IReadOnlyIndex index, params string[] propertyNames)
    {
        return index.Properties
            .Select(property => property.Name)
            .SequenceEqual(propertyNames);
    }

    private static void AssertScalarPolicies(
        IReadOnlyEntityType entityType,
        string idPropertyName,
        string namePropertyName,
        string createdAtPropertyName)
    {
        var id = entityType.FindProperty(idPropertyName);
        var name = entityType.FindProperty(namePropertyName);
        var createdAt = entityType.FindProperty(createdAtPropertyName);

        Assert.Equal(ValueGenerated.OnAdd, id!.ValueGenerated);
        Assert.False(name!.IsNullable);
        Assert.Equal(100, name.GetMaxLength());
        Assert.Equal(
            typeof(long),
            createdAt!.GetTypeMapping().Converter!.ProviderClrType);
        Assert.Equal("INTEGER", createdAt.GetColumnType());
    }
}
