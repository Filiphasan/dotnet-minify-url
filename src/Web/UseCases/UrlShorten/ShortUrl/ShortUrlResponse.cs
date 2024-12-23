namespace Web.UseCases.UrlShorten.ShortUrl;

public class ShortUrlResponse
{
    public string Token { get; set; } = null!;
    public string ShortenedUrl { get; set; } = null!;
    public string? QrCode { get; set; }
}