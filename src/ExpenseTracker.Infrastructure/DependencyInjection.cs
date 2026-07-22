using ExpenseTracker.Application.Abstractions.Persistence;
using ExpenseTracker.Infrastructure.Persistence;
using ExpenseTracker.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ExpenseTracker.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<ExpenseTrackerDbContext>(options =>
            options.UseSqlite(connectionString));
        services.AddScoped<IBudgetRepository, BudgetRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
