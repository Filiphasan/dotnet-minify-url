using Web.UseCases.UrlShorten.ShortUrl;

namespace Web.Common.Models.Endpoints.UrlShort;

public class ShortUrlRequest
{
    public string? Url { get; set; }
    public int? ExpireDay { get; set; }
    public bool HasQrCode { get; set; } = false;

    public ShortUrlCommand ToCommand()
    {
        return new ShortUrlCommand
        {
            Url = Url,
            ExpireDay = ExpireDay,
            HasQrCode = HasQrCode,
        };
    }
}