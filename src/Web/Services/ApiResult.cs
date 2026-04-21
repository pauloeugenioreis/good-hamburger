using System.Net;

namespace GoodHamburger.Web.Services;

public sealed record ApiResult<T>
{
    public bool IsSuccess { get; init; }

    public T? Value { get; init; }

    public string? ErrorMessage { get; init; }

    public HttpStatusCode StatusCode { get; init; }

    public static ApiResult<T> Success(T value, HttpStatusCode statusCode)
        => new()
        {
            IsSuccess = true,
            Value = value,
            StatusCode = statusCode
        };

    public static ApiResult<T> Failure(string errorMessage, HttpStatusCode statusCode)
        => new()
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            StatusCode = statusCode
        };
}
