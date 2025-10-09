namespace Challenge.Credit.System.Shared.Models;

public sealed class ApiResult<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<ErrorDetail>? Error { get; set; }

    public static ApiResult<T> SuccessResult(T data, string message = "Operação realizada com sucesso")
    {
        return new ApiResult<T>
        {
            Success = true,
            Message = message,
            Data = data,
            Error = []
        };
    }

    public static ApiResult<T> FailureResult(string message, List<ErrorDetail>? errors = null)
    {
        return new ApiResult<T>
        {
            Success = false,
            Message = message,
            Data = default,
            Error = errors ?? []
        };
    }

    public static ApiResult<T> FailureResult(string message, string errorCode)
    {
        return new ApiResult<T>
        {
            Success = false,
            Message = message,
            Data = default,
            Error =
            [
                new() { Code = errorCode, Message = message }
            ]
        };
    }
}

public sealed class ErrorDetail
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
