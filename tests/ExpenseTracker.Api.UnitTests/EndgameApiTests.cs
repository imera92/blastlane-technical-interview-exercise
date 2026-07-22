using ExpenseTracker.Api.Contracts.Transactions;
using ExpenseTracker.Api.Controllers;
using ExpenseTracker.Api.ErrorHandling;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.Transactions;
using ExpenseTracker.Application.Transactions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ExpenseTracker.Api.UnitTests;

public sealed class EndgameApiTests
{
    [Fact]
    public async Task TransactionCreate_ReturnsNestedCreatedAtActionResponse()
    {
        var transaction = new TransactionResult(7, "Expense", -10m, new DateOnly(2026, 7, 22), DateTimeOffset.UtcNow);
        var controller = new TransactionsController(new StubTransactionService(Result<TransactionResult>.Success(transaction)));

        var result = await controller.Create(3, new TransactionRequest { Name = "Expense", Amount = -10m, Date = transaction.Date }, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(TransactionsController.GetById), created.ActionName);
        Assert.Equal(3L, created.RouteValues!["budgetId"]);
        Assert.Equal(7L, created.RouteValues["transactionId"]);
        Assert.Equal(-10m, Assert.IsType<TransactionResponse>(created.Value).Amount);
    }

    [Fact]
    public async Task UnexpectedException_IsReturnedAsSanitizedProblemDetails()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var handler = new GlobalExceptionHandler();

        await handler.TryHandleAsync(context, new InvalidOperationException("secret database detail"), CancellationToken.None);
        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();

        Assert.Equal(500, context.Response.StatusCode);
        Assert.Contains("An unexpected error occurred.", body);
        Assert.DoesNotContain("secret database detail", body);
    }

    private sealed class StubTransactionService(Result<TransactionResult> result) : ITransactionService
    {
        public Task<Result<TransactionResult>> CreateAsync(long budgetId, CreateTransactionCommand command, CancellationToken cancellationToken) => Task.FromResult(result);
        public Task<Result<bool>> DeleteAsync(long budgetId, long transactionId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<Result<TransactionResult>> GetAsync(long budgetId, long transactionId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<Result<GroupedTransactionsResult>> ListAsync(long budgetId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<Result<TransactionResult>> UpdateAsync(long budgetId, long transactionId, UpdateTransactionCommand command, CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}
