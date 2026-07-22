using ExpenseTracker.Infrastructure.Authentication;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Persistence;

public class ExpenseTrackerDbContext
    : IdentityUserContext<ApplicationUser, Guid>
{
    public ExpenseTrackerDbContext(
        DbContextOptions<ExpenseTrackerDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(ExpenseTrackerDbContext).Assembly);
    }
}
