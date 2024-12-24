using MediatR;
using Web.Common.Models.Endpoints;

namespace Web.UseCases.UrlShorten.GetShortenedUrl;

public class GetShortenedUrlQuery : IRequest<Result<GetShortenedUrlResponse>>
{
    public string? Token { get; set; }
}