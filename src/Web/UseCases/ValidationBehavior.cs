using FluentValidation;
using MediatR;
using Web.Common.Models.Endpoints;

namespace Web.UseCases;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next();
        }

        var results = await Task.WhenAll(validators.Select(v => v.ValidateAsync(request, cancellationToken)));
        var failures = results.SelectMany(r => r.Errors).Where(f => f != null).ToList();
        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }

        return await next();
    }
}