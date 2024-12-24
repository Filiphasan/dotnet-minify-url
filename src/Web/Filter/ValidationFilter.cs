using FluentValidation;
using Web.Common.Models.Endpoints;
using Web.Extensions;

namespace Web.Filter;

public class ValidationFilter<T>(IValidator<T> validator) : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (context.Arguments.FirstOrDefault(x => x is T) is not T model)
        {
            var result = Result<object>.Invalid("Invalid Request");
            return result.ToResult();
        }

        var validationResult = await validator.ValidateAsync(model);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.GroupBy(x => x.PropertyName).ToDictionary(x => x.Key, x => x.Select(m => m.ErrorMessage).FirstOrDefault());
            var result = Result<object>.Invalid("Invalid Request", errors);
            return result.ToResult();
        }

        return await next(context);
    }
}