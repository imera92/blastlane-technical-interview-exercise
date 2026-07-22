using ExpenseTracker.Api.Contracts.Transactions;
using ExpenseTracker.Api.ErrorHandling;
using ExpenseTracker.Application.Transactions;
using ExpenseTracker.Application.Transactions.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;

[ApiController]
[Authorize]
[ProducesResponseType(typeof(ProblemDetails), 401)]
[Route("api/budgets/{budgetId:long}/transactions")]
public sealed class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    public TransactionsController(ITransactionService transactionService) => _transactionService = transactionService;

    [HttpPost]
    [ProducesResponseType(typeof(TransactionResponse), 201)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<ActionResult<TransactionResponse>> Create(long budgetId, TransactionRequest request, CancellationToken cancellationToken)
    {
        var result = await _transactionService.CreateAsync(budgetId, new CreateTransactionCommand(request.Name, request.Amount, request.Date), cancellationToken);
        if (result.IsFailure) return this.ToProblem(result.Errors);
        return CreatedAtAction(nameof(GetById), new { budgetId, transactionId = result.Value!.Id }, Map(result.Value));
    }

    [HttpGet]
    [ProducesResponseType(typeof(GroupedTransactionsResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<ActionResult<GroupedTransactionsResponse>> List(long budgetId, CancellationToken cancellationToken)
    {
        var result = await _transactionService.ListAsync(budgetId, cancellationToken);
        if (result.IsFailure) return this.ToProblem(result.Errors);
        return Ok(new GroupedTransactionsResponse(budgetId, MapGroups(result.Value!.TransactionGroups)));
    }

    [HttpGet("{transactionId:long}")]
    [ProducesResponseType(typeof(TransactionResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<ActionResult<TransactionResponse>> GetById(long budgetId, long transactionId, CancellationToken cancellationToken)
    {
        var result = await _transactionService.GetAsync(budgetId, transactionId, cancellationToken);
        return result.IsFailure ? this.ToProblem(result.Errors) : Ok(Map(result.Value!));
    }

    [HttpPut("{transactionId:long}")]
    [ProducesResponseType(typeof(TransactionResponse), 200)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<ActionResult<TransactionResponse>> Update(long budgetId, long transactionId, TransactionRequest request, CancellationToken cancellationToken)
    {
        var result = await _transactionService.UpdateAsync(budgetId, transactionId, new UpdateTransactionCommand(request.Name, request.Amount, request.Date), cancellationToken);
        return result.IsFailure ? this.ToProblem(result.Errors) : Ok(Map(result.Value!));
    }

    [HttpDelete("{transactionId:long}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> Delete(long budgetId, long transactionId, CancellationToken cancellationToken)
    {
        var result = await _transactionService.DeleteAsync(budgetId, transactionId, cancellationToken);
        return result.IsFailure ? this.ToProblem(result.Errors) : NoContent();
    }

    private static TransactionResponse Map(TransactionResult transaction) => new(transaction.Id, transaction.Name, transaction.Amount, transaction.Date, transaction.CreatedAtUtc);
    private static IReadOnlyList<TransactionGroupResponse> MapGroups(IEnumerable<TransactionGroupResult> groups) => groups.Select(group => new TransactionGroupResponse(group.Date, group.Transactions.Select(transaction => new TransactionItemResponse(transaction.Id, transaction.Name, transaction.Amount, transaction.CreatedAtUtc)).ToArray())).ToArray();
}
