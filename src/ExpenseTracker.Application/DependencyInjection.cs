using ExpenseTracker.Application.Authentication;
using ExpenseTracker.Application.Budgets;
using ExpenseTracker.Application.Transactions;
using Microsoft.Extensions.DependencyInjection;

namespace ExpenseTracker.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IBudgetService, BudgetService>();
        services.AddScoped<ITransactionService, TransactionService>();

        return services;
    }
}
