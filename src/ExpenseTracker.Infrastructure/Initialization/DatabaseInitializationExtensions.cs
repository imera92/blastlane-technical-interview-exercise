using ExpenseTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ExpenseTracker.Infrastructure.Initialization;

public static class DatabaseInitializationExtensions
{
    public static async Task InitializeDatabaseAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var scopedServices = scope.ServiceProvider;
        var dbContext = scopedServices.GetRequiredService<ExpenseTrackerDbContext>();

        await dbContext.Database.MigrateAsync(cancellationToken);

        var options = scopedServices
            .GetRequiredService<IOptions<ReviewerUserOptions>>()
            .Value;
        var seeder = scopedServices.GetRequiredService<ReviewerUserSeeder>();

        await seeder.SeedAsync(options, cancellationToken);
    }
}
