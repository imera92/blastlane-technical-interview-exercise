using ExpenseTracker.Application.Abstractions.Persistence;
using ExpenseTracker.Application.Abstractions.Security;
using ExpenseTracker.Application.Budgets;
using ExpenseTracker.Application.Budgets.Models;
using ExpenseTracker.Application.Transactions;
using ExpenseTracker.Application.Transactions.Models;
using ExpenseTracker.Domain.Budgets;
using ExpenseTracker.Domain.Transactions;
using Xunit;

namespace ExpenseTracker.Application.UnitTests;

public sealed class EndgameServiceTests
{
    [Fact]
    public async Task BudgetReads_OrderAndMapCompleteAggregate()
    {
        var userId = Guid.NewGuid();
        var older = new Budget(userId, "Older", 100m, Utc(1));
        older.AddTransaction("Later", -20m, new DateOnly(2026, 7, 23), Utc(3));
        older.AddTransaction("Earlier", 50m, new DateOnly(2026, 7, 22), Utc(2));
        var newer = new Budget(userId, "Newer", 10m, Utc(4));
        var store = new FakeStore([newer, older], older);
        var service = new BudgetService(new CurrentUser(userId), store, store, new Clock(Utc(5)));

        var list = await service.ListAsync(CancellationToken.None);
        var details = await service.GetAsync(0, CancellationToken.None);

        Assert.Equal(["Older", "Newer"], list.Value!.Select(x => x.Name));
        Assert.Equal(130m, details.Value!.CurrentBalance);
        Assert.Equal([new DateOnly(2026, 7, 22), new DateOnly(2026, 7, 23)], details.Value.TransactionGroups.Select(x => x.Date));
    }

    [Fact]
    public async Task BudgetUpdateAndDelete_MutateOwnedBudgetAndSaveOnceEach()
    {
        var userId = Guid.NewGuid();
        var budget = new Budget(userId, "Old", 100m, Utc(1));
        var store = new FakeStore([budget], budget);
        var service = new BudgetService(new CurrentUser(userId), store, store, new Clock(Utc(2)));

        var update = await service.UpdateAsync(0, new UpdateBudgetCommand(" New ", 200m), CancellationToken.None);
        var delete = await service.DeleteAsync(0, CancellationToken.None);

        Assert.True(update.IsSuccess);
        Assert.Equal("New", budget.Name);
        Assert.Equal(200m, budget.CurrentBalance);
        Assert.True(delete.IsSuccess);
        Assert.Equal(1, store.BudgetRemoveCount);
        Assert.Equal(2, store.SaveCount);
    }

    [Fact]
    public async Task TransactionOperations_CreateGroupUpdateAndDeleteOwnedTransactions()
    {
        var userId = Guid.NewGuid();
        var budget = new Budget(userId, "Budget", 1000m, Utc(1));
        var store = new FakeStore([budget], budget);
        var service = new TransactionService(new CurrentUser(userId), store, store, store, new Clock(Utc(2)));

        var created = await service.CreateAsync(0, new CreateTransactionCommand(" Expense ", -100m, new DateOnly(2026, 7, 22)), CancellationToken.None);
        store.Transaction = budget.Transactions.Single();
        var grouped = await service.ListAsync(0, CancellationToken.None);
        var updated = await service.UpdateAsync(0, 0, new UpdateTransactionCommand("Income", 250m, new DateOnly(2026, 7, 23)), CancellationToken.None);
        var deleted = await service.DeleteAsync(0, 0, CancellationToken.None);

        Assert.True(created.IsSuccess);
        Assert.Equal(900m, 1000m + created.Value!.Amount);
        Assert.Single(grouped.Value!.TransactionGroups);
        Assert.Equal(250m, updated.Value!.Amount);
        Assert.True(deleted.IsSuccess);
        Assert.Equal(1, store.TransactionRemoveCount);
        Assert.Equal(3, store.SaveCount);
    }

    [Fact]
    public async Task TransactionList_DistinguishesOwnedEmptyBudgetFromMissingBudget()
    {
        var userId = Guid.NewGuid();
        var budget = new Budget(userId, "Empty", 0m, Utc(1));
        var store = new FakeStore([budget], budget);
        var service = new TransactionService(new CurrentUser(userId), store, store, store, new Clock(Utc(2)));

        var owned = await service.ListAsync(0, CancellationToken.None);
        store.Exists = false;
        var missing = await service.ListAsync(99, CancellationToken.None);

        Assert.True(owned.IsSuccess);
        Assert.Empty(owned.Value!.TransactionGroups);
        Assert.True(missing.IsFailure);
        Assert.Equal(Application.Common.ErrorType.NotFound, Assert.Single(missing.Errors).Type);
    }

    private static DateTimeOffset Utc(int hour) => new(2026, 7, 22, hour, 0, 0, TimeSpan.Zero);
    private sealed record CurrentUser(Guid UserId) : ICurrentUser { public bool IsAuthenticated => true; }
    private sealed class Clock(DateTimeOffset now) : TimeProvider { public override DateTimeOffset GetUtcNow() => now; }

    private sealed class FakeStore : IBudgetRepository, ITransactionRepository, IUnitOfWork
    {
        private readonly IReadOnlyList<Budget> _budgets;
        private readonly Budget _budget;
        public FakeStore(IReadOnlyList<Budget> budgets, Budget budget) { _budgets = budgets; _budget = budget; }
        public bool Exists { get; set; } = true;
        public BudgetTransaction? Transaction { get; set; }
        public int SaveCount { get; private set; }
        public int BudgetRemoveCount { get; private set; }
        public int TransactionRemoveCount { get; private set; }
        public Task AddAsync(Budget budget, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<bool> ExistsForUserAsync(long budgetId, Guid userId, CancellationToken cancellationToken) => Task.FromResult(Exists && _budget.UserId == userId);
        public Task<Budget?> GetByIdForUserAsync(long budgetId, Guid userId, CancellationToken cancellationToken) => Task.FromResult(_budget.UserId == userId ? _budget : null);
        public Task<IReadOnlyList<Budget>> ListForUserAsync(Guid userId, CancellationToken cancellationToken) => Task.FromResult(_budgets);
        public void Remove(Budget budget) => BudgetRemoveCount++;
        public Task<BudgetTransaction?> GetByIdForUserAsync(long budgetId, long transactionId, Guid userId, CancellationToken cancellationToken) => Task.FromResult(Transaction);
        public Task<IReadOnlyList<BudgetTransaction>> ListForBudgetAndUserAsync(long budgetId, Guid userId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<BudgetTransaction>>(_budget.Transactions.ToArray());
        public void Remove(BudgetTransaction transaction) => TransactionRemoveCount++;
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken) { SaveCount++; return Task.FromResult(1); }
    }
}
