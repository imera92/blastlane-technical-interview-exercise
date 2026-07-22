using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using ExpenseTracker.Api.Contracts.Tasks;
using ExpenseTracker.Api.Controllers;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.Tasks;
using ExpenseTracker.Application.Tasks.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ExpenseTracker.Api.UnitTests.Controllers;

public sealed class TasksControllerTests
{
    [Fact]
    public async Task Create_WithSuccessfulCreation_ReturnsCreatedTask()
    {
        var task = new TaskResult(
            42,
            "Prepare interview",
            "Review implementation",
            "pending",
            new DateOnly(2026, 8, 15));
        var service = new StubTaskService
        {
            CreateResult = Result<TaskResult>.Success(task)
        };
        var controller = new TasksController(service);
        var request = new CreateTaskRequest
        {
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            DueDate = task.DueDate
        };
        var cancellationToken = new CancellationTokenSource().Token;

        var result = await controller.Create(request, cancellationToken);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(StatusCodes.Status201Created, created.StatusCode);
        Assert.Equal(nameof(TasksController.GetById), created.ActionName);
        Assert.Equal(42L, created.RouteValues!["taskId"]);
        var response = Assert.IsType<TaskResponse>(created.Value);
        Assert.Equal(task.Id, response.Id);
        Assert.Equal(task.Title, response.Title);
        Assert.Equal(task.Description, response.Description);
        Assert.Equal(task.Status, response.Status);
        Assert.Equal(task.DueDate, response.DueDate);
        Assert.Equal(
            new CreateTaskCommand(
                request.Title,
                request.Description,
                request.Status,
                request.DueDate!.Value),
            service.ReceivedCreateCommand);
        Assert.Equal(cancellationToken, service.ReceivedCancellationToken);
    }

    [Fact]
    public async Task Create_WithValidationFailure_ReturnsValidationProblem()
    {
        var service = new StubTaskService
        {
            CreateResult = Result<TaskResult>.Failure(
                new Error("TaskValidation", "Task title is required.", ErrorType.Validation))
        };
        var controller = new TasksController(service);

        var result = await controller.Create(
            new CreateTaskRequest
            {
                Title = "   ",
                Status = "pending",
                DueDate = new DateOnly(2026, 8, 15)
            },
            CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var problem = Assert.IsType<ValidationProblemDetails>(badRequest.Value);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
        Assert.Equal(
            "Task title is required.",
            Assert.Single(problem.Errors["TaskValidation"]));
    }

    [Fact]
    public async Task List_WithSuccessfulResult_ReturnsTasks()
    {
        var tasks = new[]
        {
            CreateResult(1, "First", "pending"),
            CreateResult(2, "Second", "completed")
        };
        var service = new StubTaskService
        {
            ListResult = Result<IReadOnlyList<TaskResult>>.Success(tasks)
        };
        var controller = new TasksController(service);

        var result = await controller.List(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<TaskResponse[]>(ok.Value);
        Assert.Equal([1L, 2L], response.Select(task => task.Id));
        Assert.Equal(["pending", "completed"], response.Select(task => task.Status));
    }

    [Fact]
    public async Task GetById_WithNotFoundResult_ReturnsNotFoundProblem()
    {
        var service = new StubTaskService
        {
            GetResult = Result<TaskResult>.Failure(
                new Error("NotFound", "The requested resource was not found.", ErrorType.NotFound))
        };
        var controller = new TasksController(service);

        var result = await controller.GetById(42, CancellationToken.None);

        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(
            StatusCodes.Status404NotFound,
            Assert.IsType<ProblemDetails>(notFound.Value).Status);
        Assert.Equal(42, service.ReceivedTaskId);
    }

    [Fact]
    public async Task Update_WithSuccessfulResult_ReturnsUpdatedTask()
    {
        var task = CreateResult(42, "Updated", "inProgress");
        var service = new StubTaskService
        {
            UpdateResult = Result<TaskResult>.Success(task)
        };
        var controller = new TasksController(service);
        var request = new UpdateTaskRequest
        {
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            DueDate = task.DueDate
        };

        var result = await controller.Update(42, request, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal("inProgress", Assert.IsType<TaskResponse>(ok.Value).Status);
        Assert.Equal(
            new UpdateTaskCommand(
                request.Title,
                request.Description,
                request.Status,
                request.DueDate!.Value),
            service.ReceivedUpdateCommand);
        Assert.Equal(42, service.ReceivedTaskId);
    }

    [Fact]
    public async Task Delete_WithSuccessfulResult_ReturnsNoContent()
    {
        var service = new StubTaskService
        {
            DeleteResult = Result<bool>.Success(true)
        };
        var controller = new TasksController(service);

        var result = await controller.Delete(42, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(42, service.ReceivedTaskId);
    }

    [Fact]
    public void Controller_HasAuthorizeMetadata()
    {
        var attributes = typeof(TasksController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true);

        Assert.NotEmpty(attributes);
    }

    [Fact]
    public void CreateRequest_WithApprovedValues_PassesDataAnnotationValidation()
    {
        var request = new CreateTaskRequest
        {
            Title = "Task",
            Description = null,
            Status = "inProgress",
            DueDate = new DateOnly(2026, 8, 15)
        };

        Assert.True(IsValid(request));
    }

    [Fact]
    public void CreateRequest_WithInvalidFields_FailsDataAnnotationValidation()
    {
        var request = new CreateTaskRequest
        {
            Title = new string('a', 101),
            Description = new string('b', 1001),
            Status = "invalid",
            DueDate = null
        };

        Assert.False(IsValid(request));
    }

    [Fact]
    public void UpdateRequest_WithInvalidFields_FailsDataAnnotationValidation()
    {
        var request = new UpdateTaskRequest
        {
            Title = string.Empty,
            Description = new string('b', 1001),
            Status = "Pending",
            DueDate = null
        };

        Assert.False(IsValid(request));
    }

    [Fact]
    public void TaskResponse_UsesCamelCaseDueDateAndDoesNotExposeUserId()
    {
        var response = new TaskResponse(
            42,
            "Task",
            string.Empty,
            "pending",
            new DateOnly(2026, 8, 15));

        var json = JsonSerializer.Serialize(
            response,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.Contains("\"dueDate\":", json);
        Assert.DoesNotContain("due_date", json);
        Assert.DoesNotContain("userId", json);
    }

    private static TaskResult CreateResult(long id, string title, string status) => new(
        id,
        title,
        "Description",
        status,
        new DateOnly(2026, 8, 15));

    private static bool IsValid(object value)
    {
        var results = new List<ValidationResult>();
        return Validator.TryValidateObject(
            value,
            new ValidationContext(value),
            results,
            validateAllProperties: true);
    }

    private sealed class StubTaskService : ITaskService
    {
        public Result<TaskResult>? CreateResult { get; init; }
        public Result<bool>? DeleteResult { get; init; }
        public Result<TaskResult>? GetResult { get; init; }
        public Result<IReadOnlyList<TaskResult>>? ListResult { get; init; }
        public Result<TaskResult>? UpdateResult { get; init; }
        public CreateTaskCommand? ReceivedCreateCommand { get; private set; }
        public UpdateTaskCommand? ReceivedUpdateCommand { get; private set; }
        public long ReceivedTaskId { get; private set; }
        public CancellationToken ReceivedCancellationToken { get; private set; }

        public Task<Result<TaskResult>> CreateAsync(
            CreateTaskCommand command,
            CancellationToken cancellationToken)
        {
            ReceivedCreateCommand = command;
            ReceivedCancellationToken = cancellationToken;
            return Task.FromResult(CreateResult!);
        }

        public Task<Result<bool>> DeleteAsync(long taskId, CancellationToken cancellationToken)
        {
            ReceivedTaskId = taskId;
            ReceivedCancellationToken = cancellationToken;
            return Task.FromResult(DeleteResult!);
        }

        public Task<Result<TaskResult>> GetAsync(long taskId, CancellationToken cancellationToken)
        {
            ReceivedTaskId = taskId;
            ReceivedCancellationToken = cancellationToken;
            return Task.FromResult(GetResult!);
        }

        public Task<Result<IReadOnlyList<TaskResult>>> ListAsync(CancellationToken cancellationToken)
        {
            ReceivedCancellationToken = cancellationToken;
            return Task.FromResult(ListResult!);
        }

        public Task<Result<TaskResult>> UpdateAsync(long taskId, UpdateTaskCommand command, CancellationToken cancellationToken)
        {
            ReceivedTaskId = taskId;
            ReceivedUpdateCommand = command;
            ReceivedCancellationToken = cancellationToken;
            return Task.FromResult(UpdateResult!);
        }
    }
}
