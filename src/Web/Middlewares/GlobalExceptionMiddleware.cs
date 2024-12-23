using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Web.Models.Endpoints;

namespace Web.Middlewares;

public class GlobalExceptionMiddleware(ILogger<GlobalExceptionMiddleware> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "An unhandled exception has occurred while executing the request");

        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        var response = Result<object>.Error(500, "An unhandled exception has occurred while executing the request");
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}