using ExpenseTracker.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.ErrorHandling;

public static class ResultExtensions
{
    public static ObjectResult ToProblem(this ControllerBase controller, IReadOnlyList<Error> errors)
    {
        if (errors.Any(error => error.Type == ErrorType.Unauthorized))
            return controller.Unauthorized(new ProblemDetails { Status = 401, Title = "Authentication is required." });
        if (errors.Any(error => error.Type == ErrorType.NotFound))
            return controller.NotFound(new ProblemDetails { Status = 404, Title = "The requested resource was not found." });
        if (errors.Any(error => error.Type == ErrorType.Conflict))
            return controller.Conflict(new ProblemDetails { Status = 409, Title = "The request conflicts with existing data." });

        var details = errors.GroupBy(error => error.Code).ToDictionary(group => group.Key, group => group.Select(error => error.Description).ToArray());
        return controller.BadRequest(new ValidationProblemDetails(details) { Status = 400, Title = "One or more validation errors occurred." });
    }
}
