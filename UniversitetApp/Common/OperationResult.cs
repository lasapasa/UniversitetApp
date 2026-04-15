namespace UniversitetApp;

public class OperationResult
{
    public bool IsSuccess { get; }
    public string Message { get; }
    public string? ErrorCode { get; }

    protected OperationResult(bool isSuccess, string message, string? errorCode = null)
    {
        IsSuccess = isSuccess;
        Message = message;
        ErrorCode = errorCode;
    }

    public static OperationResult Success(string message) => new(true, message);
    public static OperationResult Failure(string message, string? errorCode = null) => new(false, message, errorCode);
}

public class OperationResult<T> : OperationResult
{
    public T? Data { get; }

    private OperationResult(bool isSuccess, string message, T? data = default, string? errorCode = null)
        : base(isSuccess, message, errorCode)
    {
        Data = data;
    }

    public static OperationResult<T> Success(string message, T data) => new(true, message, data);
    public static new OperationResult<T> Failure(string message, string? errorCode = null) => new(false, message, default, errorCode);
}
