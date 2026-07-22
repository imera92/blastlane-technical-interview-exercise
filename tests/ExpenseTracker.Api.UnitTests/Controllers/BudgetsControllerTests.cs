using System.ComponentModel.DataAnnotations;
using ExpenseTracker.Api.Contracts.Budgets;
using ExpenseTracker.Api.Controllers;
using ExpenseTracker.Application.Budgets;
using ExpenseTracker.Application.Budgets.Models;
using ExpenseTracker.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ExpenseTracker.Api.UnitTests.Controllers;

public sealed class BudgetsControllerTests
{
    [Fact]
    public async Task Create_WithSuccessfulCreation_ReturnsCreatedBudget()
    {
        var createdAtUtc = new DateTimeOffset(2026, 7, 22, 12, 0, 0, TimeSpan.Zero);
        var budget = new BudgetResult(
            42,
            "Monthly expenses",
            1000.25m,
            1000.25m,
            createdAtUtc);
        var budgetService = new StubBudgetService(Result<BudgetResult>.Success(budget));
        var controller = new BudgetsController(budgetService);
        var request = new CreateBudgetRequest
        {
            Name = "Monthly expenses",
            StartingBalance = 1000.25m
        };
        var cancellationToken = new CancellationTokenSource().Token;

        var result = await controller.Create(request, cancellationToken);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(StatusCodes.Status201Created, created.StatusCode);
        Assert.Equal(nameof(BudgetsController.GetById), created.ActionName);
        Assert.Equal(42L, created.RouteValues!["budgetId"]);
        var response = Assert.IsType<BudgetSummaryResponse>(created.Value);
        Assert.Equal(budget.Id, response.Id);
        Assert.Equal(budget.Name, response.Name);
        Assert.Equal(budget.StartingBalance, response.StartingBalance);
        Assert.Equal(budget.CurrentBalance, response.CurrentBalance);
        Assert.Equal(budget.CreatedAtUtc, response.CreatedAtUtc);
        Assert.Equal(
            new CreateBudgetCommand(request.Name, request.StartingBalance),
            budgetService.ReceivedCommand);
        Assert.Equal(cancellationToken, budgetService.ReceivedCancellationToken);
    }

    [Fact]
    public async Task Create_WithValidationFailure_ReturnsValidationProblem()
    {
        var error = new Error(
            "BudgetValidation",
            "Budget name is required.",
            ErrorType.Validation);
        var budgetService = new StubBudgetService(
            Result<BudgetResult>.Failure(error));
        var controller = new BudgetsController(budgetService);

        var result = await controller.Create(
            new CreateBudgetRequest
            {
                Name = "   ",
                StartingBalance = 1000m
            },
            CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var problem = Assert.IsType<ValidationProblemDetails>(badRequest.Value);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
        Assert.Equal(error.Description, Assert.Single(problem.Errors[error.Code]));
    }

    [Fact]
    public async Task Create_WithUnauthorizedResult_ReturnsUnauthorizedProblem()
    {
        var budgetService = new StubBudgetService(
            Result<BudgetResult>.Failure(
                new Error(
                    "Unauthorized",
                    "Authentication is required.",
                    ErrorType.Unauthorized)));
        var controller = new BudgetsController(budgetService);

        var result = await controller.Create(
            new CreateBudgetRequest
            {
                Name = "Monthly expenses",
                StartingBalance = 1000m
            },
            CancellationToken.None);

        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        var problem = Assert.IsType<ProblemDetails>(unauthorized.Value);
        Assert.Equal(StatusCodes.Status401Unauthorized, problem.Status);
        Assert.Equal("Authentication is required.", problem.Title);
    }

    [Fact]
    public void Controller_HasAuthorizeMetadata()
    {
        var attributes = typeof(BudgetsController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true);

        Assert.NotEmpty(attributes);
    }

    [Fact]
    public void CreateBudgetRequest_WithInvalidName_FailsDataAnnotationValidation()
    {
        var request = new CreateBudgetRequest
        {
            Name = new string('a', 101),
            StartingBalance = 1000m
        };
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            validationResults,
            validateAllProperties: true);

        Assert.False(isValid);
        Assert.Single(validationResults);
    }

    [Fact]
    public void CreateBudgetRequest_WithMissingName_FailsDataAnnotationValidation()
    {
        var request = new CreateBudgetRequest
        {
            Name = string.Empty,
            StartingBalance = 1000m
        };
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            validationResults,
            validateAllProperties: true);

        Assert.False(isValid);
        Assert.NotEmpty(validationResults);
    }

    private sealed class StubBudgetService : IBudgetService
    {
        private readonly Result<BudgetResult> _result;

        public StubBudgetService(Result<BudgetResult> result)
        {
            _result = result;
        }

        public CreateBudgetCommand? ReceivedCommand { get; private set; }
        public CancellationToken ReceivedCancellationToken { get; private set; }

        public Task<Result<BudgetResult>> CreateAsync(
            CreateBudgetCommand command,
            CancellationToken cancellationToken)
        {
            ReceivedCommand = command;
            ReceivedCancellationToken = cancellationToken;

            return Task.FromResult(_result);
        }

        public Task<Result<bool>> DeleteAsync(long budgetId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<Result<BudgetDetailsResult>> GetAsync(long budgetId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<Result<IReadOnlyList<BudgetResult>>> ListAsync(CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<Result<BudgetResult>> UpdateAsync(long budgetId, UpdateBudgetCommand command, CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}
