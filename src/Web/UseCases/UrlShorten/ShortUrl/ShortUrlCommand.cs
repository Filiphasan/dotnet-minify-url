using MediatR;
using Web.Models.Endpoints;

namespace Web.UseCases.UrlShorten.ShortUrl;

public class ShortUrlCommand : IRequest<Result<ShortUrlResponse>>
{
    public string? Url { get; set; }
    public int ExpireDay { get; set; }
    public bool HasQrCode { get; set; }
}