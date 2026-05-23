namespace LMS.DTOs;

public enum ErrorType
{
    NotFound,
    Unauthorized,
    Conflict,
    ValidationError
}

public class ServiceResult
{
    public bool IsSuccess { get; }
    public ErrorType? Error { get; }
    public string? ErrorMessage { get; }

    protected ServiceResult(bool isSuccess, ErrorType? error, string? errorMessage)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorMessage = errorMessage;
    }

    public static ServiceResult Success() => new(true, null, null);

    public static ServiceResult Failure(ErrorType error, string message) =>
        new(false, error, message);

    public static ServiceResult<T> Success<T>(T value) => new(value);

    public static ServiceResult<T> Failure<T>(ErrorType error, string message) =>
        new(error, message);
}

public class ServiceResult<T> : ServiceResult
{
    public T? Value { get; }

    internal ServiceResult(T value) : base(true, null, null)
    {
        Value = value;
    }

    internal ServiceResult(ErrorType error, string message) : base(false, error, message)
    {
        Value = default;
    }
}
