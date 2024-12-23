using MediatR;
using Web.Models.Endpoints;

namespace Web.UseCases.UrlShorten.GetShortenedUrl;

public class GetShortenedUrlQuery : IRequest<Result<GetShortenedUrlResponse>>
{
    public string? Token { get; set; }
}