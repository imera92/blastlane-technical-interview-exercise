using ExpenseTracker.Domain.Common;

namespace ExpenseTracker.Domain.Tasks;

public class TaskItem
{
    public long Id { get; private set; }
    public Guid UserId { get; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public TaskStatus Status { get; private set; }
    public DateOnly DueDate { get; private set; }

    public TaskItem(
        Guid userId,
        string title,
        string? description,
        TaskStatus status,
        DateOnly dueDate)
    {
        UserId = userId;
        Title = ValidateTitle(title);
        Description = ValidateDescription(description);
        Status = ValidateStatus(status);
        DueDate = dueDate;
    }

    public void Update(
        string title,
        string? description,
        TaskStatus status,
        DateOnly dueDate)
    {
        var validatedTitle = ValidateTitle(title);
        var validatedDescription = ValidateDescription(description);
        var validatedStatus = ValidateStatus(status);

        Title = validatedTitle;
        Description = validatedDescription;
        Status = validatedStatus;
        DueDate = dueDate;
    }

    private static string ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainValidationException("Task title is required.");
        }

        var trimmedTitle = title.Trim();

        if (trimmedTitle.Length > 100)
        {
            throw new DomainValidationException("Task title cannot exceed 100 characters.");
        }

        return trimmedTitle;
    }

    private static string ValidateDescription(string? description)
    {
        var trimmedDescription = description?.Trim() ?? string.Empty;

        if (trimmedDescription.Length > 1000)
        {
            throw new DomainValidationException("Task description cannot exceed 1000 characters.");
        }

        return trimmedDescription;
    }

    private static TaskStatus ValidateStatus(TaskStatus status)
    {
        if (!Enum.IsDefined(status))
        {
            throw new DomainValidationException("Task status is invalid.");
        }

        return status;
    }
}
