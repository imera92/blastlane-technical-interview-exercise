using ExpenseTracker.Application.Abstractions.Persistence;
using ExpenseTracker.Application.Abstractions.Security;
using ExpenseTracker.Application.Budgets;
using ExpenseTracker.Application.Budgets.Models;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Domain.Budgets;
using Xunit;

namespace ExpenseTracker.Application.UnitTests.Budgets;

public sealed class BudgetServiceTests
{
    [Fact]
    public async Task CreateAsync_WithValidCommand_CreatesOwnedBudgetAndSavesOnce()
    {
        var userId = Guid.NewGuid();
        var createdAtUtc = new DateTimeOffset(2026, 7, 22, 12, 0, 0, TimeSpan.Zero);
        var operations = new List<string>();
        var repository = new RecordingBudgetRepository(operations);
        var unitOfWork = new RecordingUnitOfWork(operations);
        var service = new BudgetService(
            new StubCurrentUser(isAuthenticated: true, userId),
            repository,
            unitOfWork,
            new StubTimeProvider(createdAtUtc));
        var command = new CreateBudgetCommand(
            "  Monthly expenses  ",
            1000.25m);
        var cancellationToken = new CancellationTokenSource().Token;

        var result = await service.CreateAsync(command, cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value!.Id);
        Assert.Equal("Monthly expenses", result.Value.Name);
        Assert.Equal(1000.25m, result.Value.StartingBalance);
        Assert.Equal(1000.25m, result.Value.CurrentBalance);
        Assert.Equal(createdAtUtc, result.Value.CreatedAtUtc);

        var budget = Assert.IsType<Budget>(repository.AddedBudget);
        Assert.Equal(userId, budget.UserId);
        Assert.Equal(createdAtUtc, budget.CreatedAtUtc);
        Assert.Equal(cancellationToken, repository.ReceivedCancellationToken);
        Assert.Equal(1, repository.AddCallCount);
        Assert.Equal(1, unitOfWork.SaveCallCount);
        Assert.Equal(cancellationToken, unitOfWork.ReceivedCancellationToken);
        Assert.Equal(["Add", "Save"], operations);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidName_ReturnsValidationWithoutSaving()
    {
        await AssertValidationFailureWithoutSavingAsync("   ", 1000m);
    }

    [Fact]
    public async Task CreateAsync_WithOverlongName_ReturnsValidationWithoutSaving()
    {
        await AssertValidationFailureWithoutSavingAsync(new string('a', 101), 1000m);
    }

    [Fact]
    public async Task CreateAsync_WithNegativeStartingBalance_ReturnsValidationWithoutSaving()
    {
        await AssertValidationFailureWithoutSavingAsync("Monthly expenses", -0.01m);
    }

    [Fact]
    public async Task CreateAsync_WithScaleThreeStartingBalance_ReturnsValidationWithoutSaving()
    {
        await AssertValidationFailureWithoutSavingAsync("Monthly expenses", 1.230m);
    }

    [Fact]
    public async Task CreateAsync_WithoutAuthenticatedUser_ReturnsUnauthorizedWithoutSaving()
    {
        var operations = new List<string>();
        var repository = new RecordingBudgetRepository(operations);
        var unitOfWork = new RecordingUnitOfWork(operations);
        var service = new BudgetService(
            new StubCurrentUser(isAuthenticated: false, Guid.Empty),
            repository,
            unitOfWork,
            new StubTimeProvider(DateTimeOffset.UtcNow));

        var result = await service.CreateAsync(
            new CreateBudgetCommand("Monthly expenses", 1000m),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Errors);
        Assert.Equal("Unauthorized", error.Code);
        Assert.Equal(ErrorType.Unauthorized, error.Type);
        Assert.Equal(0, repository.AddCallCount);
        Assert.Equal(0, unitOfWork.SaveCallCount);
        Assert.Empty(operations);
    }

    private static async Task AssertValidationFailureWithoutSavingAsync(
        string name,
        decimal startingBalance)
    {
        var operations = new List<string>();
        var repository = new RecordingBudgetRepository(operations);
        var unitOfWork = new RecordingUnitOfWork(operations);
        var service = new BudgetService(
            new StubCurrentUser(isAuthenticated: true, Guid.NewGuid()),
            repository,
            unitOfWork,
            new StubTimeProvider(DateTimeOffset.UtcNow));

        var result = await service.CreateAsync(
            new CreateBudgetCommand(name, startingBalance),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Errors);
        Assert.Equal("BudgetValidation", error.Code);
        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.Equal(0, repository.AddCallCount);
        Assert.Equal(0, unitOfWork.SaveCallCount);
        Assert.Empty(operations);
    }

    private sealed class StubCurrentUser : ICurrentUser
    {
        public StubCurrentUser(bool isAuthenticated, Guid userId)
        {
            IsAuthenticated = isAuthenticated;
            UserId = userId;
        }

        public bool IsAuthenticated { get; }
        public Guid UserId { get; }
    }

    private sealed class StubTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public StubTimeProvider(DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }

        public override DateTimeOffset GetUtcNow()
        {
            return _utcNow;
        }
    }

    private sealed class RecordingBudgetRepository : IBudgetRepository
    {
        private readonly List<string> _operations;

        public RecordingBudgetRepository(List<string> operations)
        {
            _operations = operations;
        }

        public int AddCallCount { get; private set; }
        public Budget? AddedBudget { get; private set; }
        public CancellationToken ReceivedCancellationToken { get; private set; }

        public Task<bool> ExistsForUserAsync(long budgetId, Guid userId, CancellationToken cancellationToken)
        {
            return Task.FromResult(AddedBudget?.Id == budgetId && AddedBudget.UserId == userId);
        }

        public Task AddAsync(Budget budget, CancellationToken cancellationToken)
        {
            AddCallCount++;
            AddedBudget = budget;
            ReceivedCancellationToken = cancellationToken;
            _operations.Add("Add");

            return Task.CompletedTask;
        }

        public Task<Budget?> GetByIdForUserAsync(
            long budgetId,
            Guid userId,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyList<Budget>> ListForUserAsync(
            Guid userId,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public void Remove(Budget budget)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class RecordingUnitOfWork : IUnitOfWork
    {
        private readonly List<string> _operations;

        public RecordingUnitOfWork(List<string> operations)
        {
            _operations = operations;
        }

        public int SaveCallCount { get; private set; }
        public CancellationToken ReceivedCancellationToken { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveCallCount++;
            ReceivedCancellationToken = cancellationToken;
            _operations.Add("Save");

            return Task.FromResult(1);
        }
    }
}
