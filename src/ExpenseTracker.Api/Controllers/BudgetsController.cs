using ExpenseTracker.Api.Contracts.Budgets;
using ExpenseTracker.Api.Contracts.Transactions;
using ExpenseTracker.Api.ErrorHandling;
using ExpenseTracker.Application.Budgets;
using ExpenseTracker.Application.Budgets.Models;
using ExpenseTracker.Application.Transactions.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;

[ApiController]
[Authorize]
[ProducesResponseType(typeof(ProblemDetails), 401)]
[Route("api/budgets")]
public sealed class BudgetsController : ControllerBase
{
    private readonly IBudgetService _budgetService;
    public BudgetsController(IBudgetService budgetService) => _budgetService = budgetService;

    [HttpPost]
    [ProducesResponseType(typeof(BudgetSummaryResponse), 201)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    public async Task<ActionResult<BudgetSummaryResponse>> Create(CreateBudgetRequest request, CancellationToken cancellationToken)
    {
        var result = await _budgetService.CreateAsync(new CreateBudgetCommand(request.Name, request.StartingBalance), cancellationToken);
        if (result.IsFailure) return this.ToProblem(result.Errors);
        return CreatedAtAction(nameof(GetById), new { budgetId = result.Value!.Id }, MapSummary(result.Value));
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BudgetSummaryResponse>), 200)]
    public async Task<ActionResult<IReadOnlyList<BudgetSummaryResponse>>> List(CancellationToken cancellationToken)
    {
        var result = await _budgetService.ListAsync(cancellationToken);
        if (result.IsFailure) return this.ToProblem(result.Errors);
        return Ok(result.Value!.Select(MapSummary).ToArray());
    }

    [HttpGet("{budgetId:long}")]
    [ProducesResponseType(typeof(BudgetDetailsResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<ActionResult<BudgetDetailsResponse>> GetById(long budgetId, CancellationToken cancellationToken)
    {
        var result = await _budgetService.GetAsync(budgetId, cancellationToken);
        if (result.IsFailure) return this.ToProblem(result.Errors);
        var budget = result.Value!;
        return Ok(new BudgetDetailsResponse(budget.Id, budget.Name, budget.StartingBalance, budget.CurrentBalance, budget.CreatedAtUtc, MapGroups(budget.TransactionGroups)));
    }

    [HttpPut("{budgetId:long}")]
    [ProducesResponseType(typeof(BudgetSummaryResponse), 200)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<ActionResult<BudgetSummaryResponse>> Update(long budgetId, UpdateBudgetRequest request, CancellationToken cancellationToken)
    {
        var result = await _budgetService.UpdateAsync(budgetId, new UpdateBudgetCommand(request.Name, request.StartingBalance), cancellationToken);
        if (result.IsFailure) return this.ToProblem(result.Errors);
        return Ok(MapSummary(result.Value!));
    }

    [HttpDelete("{budgetId:long}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> Delete(long budgetId, CancellationToken cancellationToken)
    {
        var result = await _budgetService.DeleteAsync(budgetId, cancellationToken);
        return result.IsFailure ? this.ToProblem(result.Errors) : NoContent();
    }

    private static BudgetSummaryResponse MapSummary(BudgetResult budget) => new(budget.Id, budget.Name, budget.StartingBalance, budget.CurrentBalance, budget.CreatedAtUtc);
    private static IReadOnlyList<TransactionGroupResponse> MapGroups(IEnumerable<TransactionGroupResult> groups) => groups.Select(group => new TransactionGroupResponse(group.Date, group.Transactions.Select(transaction => new TransactionItemResponse(transaction.Id, transaction.Name, transaction.Amount, transaction.CreatedAtUtc)).ToArray())).ToArray();
}
