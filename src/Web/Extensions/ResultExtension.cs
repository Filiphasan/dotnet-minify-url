using Web.Common.Models.Endpoints;

namespace Web.Extensions;

public static class ResultExtension
{
    public static IResult ToResult<T>(this Result<T> result) where T : class
    {
        return Results.Json(result, statusCode: result.StatusCode);
    }
}