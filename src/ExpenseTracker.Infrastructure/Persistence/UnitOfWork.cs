using ExpenseTracker.Application.Abstractions.Persistence;

namespace ExpenseTracker.Infrastructure.Persistence;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly ExpenseTrackerDbContext _dbContext;

    public UnitOfWork(ExpenseTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
