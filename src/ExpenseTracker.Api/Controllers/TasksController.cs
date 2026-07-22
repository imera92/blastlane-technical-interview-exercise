using ExpenseTracker.Api.Contracts.Tasks;
using ExpenseTracker.Api.ErrorHandling;
using ExpenseTracker.Application.Tasks;
using ExpenseTracker.Application.Tasks.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;

[ApiController]
[Authorize]
[ProducesResponseType(typeof(ProblemDetails), 401)]
[Route("api/tasks")]
public sealed class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(TaskResponse), 201)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    public async Task<ActionResult<TaskResponse>> Create(
        CreateTaskRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _taskService.CreateAsync(
            new CreateTaskCommand(
                request.Title,
                request.Description,
                request.Status,
                request.DueDate!.Value),
            cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblem(result.Errors);
        }

        return CreatedAtAction(
            nameof(GetById),
            new { taskId = result.Value!.Id },
            Map(result.Value));
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TaskResponse>), 200)]
    public async Task<ActionResult<IReadOnlyList<TaskResponse>>> List(
        CancellationToken cancellationToken)
    {
        var result = await _taskService.ListAsync(cancellationToken);

        return result.IsFailure
            ? this.ToProblem(result.Errors)
            : Ok(result.Value!.Select(Map).ToArray());
    }

    [HttpGet("{taskId:long}")]
    [ProducesResponseType(typeof(TaskResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<ActionResult<TaskResponse>> GetById(
        long taskId,
        CancellationToken cancellationToken)
    {
        var result = await _taskService.GetAsync(taskId, cancellationToken);

        return result.IsFailure
            ? this.ToProblem(result.Errors)
            : Ok(Map(result.Value!));
    }

    [HttpPut("{taskId:long}")]
    [ProducesResponseType(typeof(TaskResponse), 200)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<ActionResult<TaskResponse>> Update(
        long taskId,
        UpdateTaskRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _taskService.UpdateAsync(
            taskId,
            new UpdateTaskCommand(
                request.Title,
                request.Description,
                request.Status,
                request.DueDate!.Value),
            cancellationToken);

        return result.IsFailure
            ? this.ToProblem(result.Errors)
            : Ok(Map(result.Value!));
    }

    [HttpDelete("{taskId:long}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> Delete(
        long taskId,
        CancellationToken cancellationToken)
    {
        var result = await _taskService.DeleteAsync(taskId, cancellationToken);

        return result.IsFailure
            ? this.ToProblem(result.Errors)
            : NoContent();
    }

    private static TaskResponse Map(TaskResult task) => new(
        task.Id,
        task.Title,
        task.Description,
        task.Status,
        task.DueDate);
}
