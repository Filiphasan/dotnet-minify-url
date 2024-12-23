using Carter;
using MediatR;
using Web.Extensions;
using Web.Filter;
using Web.Models.Endpoints;
using Web.Models.Endpoints.UrlShort;
using Web.UseCases.UrlShorten.GetShortenedUrl;
using Web.UseCases.UrlShorten.ShortUrl;

namespace Web.Endpoints;

public class UrlShortEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/url-shorts")
            .WithTags("Url Shorten Endpoint");

        group.MapPost("", ShortUrlAsync)
            .Produces<Result<ShortUrlResponse>>()
            .Produces<Result<object>>(400)
            .AddEndpointFilter<ValidationFilter<ShortUrlRequest>>();

        app.MapGet("/{token}", GetShortenedUrlAsync)
            .WithTags("Url Shorten Endpoint");
    }

    private static async Task<IResult> ShortUrlAsync(ShortUrlRequest request, ISender sender)
    {
        var command = request.ToCommand();
        var result = await sender.Send(command);
        return result.ToResult();
    }
    
    private static async Task<IResult> GetShortenedUrlAsync(string? token, ISender sender)
    {
        var result = await sender.Send(new GetShortenedUrlQuery { Token = token });
        return result.StatusCode == 200 ? Results.Redirect(result.Data!.LongUrl!) : result.ToResult();
    }
}