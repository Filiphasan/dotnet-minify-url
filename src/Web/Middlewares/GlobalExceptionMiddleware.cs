using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Web.Common.Models.Endpoints;

namespace Web.Middlewares;

public class GlobalExceptionMiddleware(ILogger<GlobalExceptionMiddleware> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "An unhandled exception has occurred while executing the request");

        httpContext.Response.ContentType = "application/json";

        Result<object> response;
        if (exception is ValidationException validationException)
        {
            var errors = validationException.Errors.GroupBy(x => x.PropertyName).ToDictionary(x => x.Key, x => x.Select(y => y.ErrorMessage).FirstOrDefault());
            response = Result<object>.Invalid("Invalid", errors);
        }
        else
        {
            response = Result<object>.Error(500, "An unhandled exception has occurred while executing the request");
        }

        httpContext.Response.StatusCode = response.StatusCode;
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}