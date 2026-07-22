using ExpenseTracker.Api.Contracts.Budgets;
using ExpenseTracker.Application.Budgets;
using ExpenseTracker.Application.Budgets.Models;
using ExpenseTracker.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/budgets")]
public sealed class BudgetsController : ControllerBase
{
    private readonly IBudgetService _budgetService;

    public BudgetsController(IBudgetService budgetService)
    {
        _budgetService = budgetService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(BudgetSummaryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BudgetSummaryResponse>> Create(
        CreateBudgetRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _budgetService.CreateAsync(
            new CreateBudgetCommand(request.Name, request.StartingBalance),
            cancellationToken);

        if (result.IsFailure)
        {
            if (result.Errors.Any(error => error.Type == ErrorType.Unauthorized))
            {
                return Unauthorized(new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Authentication is required."
                });
            }

            var errors = result.Errors
                .GroupBy(error => error.Code)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.Description).ToArray());

            return BadRequest(new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred."
            });
        }

        var budget = result.Value!;
        var response = new BudgetSummaryResponse(
            budget.Id,
            budget.Name,
            budget.StartingBalance,
            budget.CurrentBalance,
            budget.CreatedAtUtc);

        return Created($"/api/budgets/{budget.Id}", response);
    }
}
