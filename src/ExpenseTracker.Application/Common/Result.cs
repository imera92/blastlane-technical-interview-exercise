namespace ExpenseTracker.Application.Common;

public sealed class Result<T>
{
    private Result(bool isSuccess, T? value, IReadOnlyList<Error> errors)
    {
        IsSuccess = isSuccess;
        Value = value;
        Errors = errors;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public IReadOnlyList<Error> Errors { get; }

    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value, []);
    }

    public static Result<T> Failure(params Error[] errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        if (errors.Length == 0)
        {
            throw new ArgumentException(
                "A failed result must contain at least one error.",
                nameof(errors));
        }

        return new Result<T>(false, default, errors);
    }
}
