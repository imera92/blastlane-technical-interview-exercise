using ExpenseTracker.Application;
using ExpenseTracker.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' was not found.");

builder.Services.AddControllers();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);

var app = builder.Build();

app.MapControllers();

app.Run();
