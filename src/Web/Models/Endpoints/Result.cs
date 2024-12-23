namespace Web.Models.Endpoints;

public class Result<T> where T : class
{
    public int StatusCode { get; private init; }
    public string? Message { get; private init; }
    public T? Data { get; private init; }
    public Dictionary<string, string?>? Errors { get; private init; }

    private Result()
    {
    }

    public static Result<T> Success(T data)
    {
        return new Result<T>
        {
            StatusCode = 200,
            Message = "Success",
            Data = data,
            Errors = null,
        };
    }

    public static Result<T> Error(int statusCode, string? message)
    {
        return new Result<T>
        {
            StatusCode = statusCode,
            Message = message,
            Data = null,
            Errors = null,
        };
    }

    public static Result<T> Error<TOther>(Result<TOther> result) where TOther : class
    {
        return new Result<T>
        {
            StatusCode = result.StatusCode,
            Message = result.Message,
            Data = null,
            Errors = result.Errors,
        };
    }

    public static Result<T> Invalid(string? message)
    {
        return new Result<T>
        {
            StatusCode = 400,
            Message = message,
            Data = null
        };
    }

    public static Result<T> Invalid(string? message, Dictionary<string, string?> messages)
    {
        return new Result<T>
        {
            StatusCode = 400,
            Message = message,
            Errors = messages,
            Data = null
        };
    }
}