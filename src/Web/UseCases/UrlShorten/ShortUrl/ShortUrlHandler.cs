using MediatR;
using MongoDB.Driver;
using QRCoder;
using Web.Constants;
using Web.Data;
using Web.Models.Endpoints;
using Web.Services.Interfaces;
using Web.UseCases.UrlToken.GetUnusedToken;

namespace Web.UseCases.UrlShorten.ShortUrl;

public class ShortUrlHandler(ISender sender, MongoDbContext dbContext, ICacheService cacheService)
    : IRequestHandler<ShortUrlCommand, Result<ShortUrlResponse>>
{
    public async Task<Result<ShortUrlResponse>> Handle(ShortUrlCommand request, CancellationToken cancellationToken)
    {
        var response = new ShortUrlResponse();
        string token;
        var urlExist = await dbContext.UrlShortens.Find(x => x.Url == request.Url).FirstOrDefaultAsync(cancellationToken);
        if (urlExist != null)
        {
            token = urlExist.Token;
        }
        else
        {
            var getTokenResult = await sender.Send(new GetUnusedTokenQuery(), cancellationToken);
            if (getTokenResult.StatusCode != 200)
            {
                return Result<ShortUrlResponse>.Error(getTokenResult);
            }

            token = getTokenResult.Data!.Token!;
        }

        response.Token = token;
        response.ShortenedUrl = $"https://localhost:5001/{token}";
        response.QrCode = GetQrBase64(request, response.ShortenedUrl);
        var urlShorten = new Data.Entities.UrlShorten
        {
            Url = request.Url!,
            Token = token,
            CreatedAt = DateTime.UtcNow,
            ExpiredAt = DateTime.UtcNow.AddDays(request.ExpireDay)
        };
        await dbContext.UrlShortens.InsertOneAsync(urlShorten, null, cancellationToken);
        await cacheService.SetAsync(string.Format(RedisConstant.Key.ShortUrl, token), request.Url!, TimeSpan.FromDays(request.ExpireDay));

        return Result<ShortUrlResponse>.Success(response);
    }

    private static string? GetQrBase64(ShortUrlCommand request, string url)
    {
        if (!request.HasQrCode)
        {
            return null;
        }

        var qrGenerator = new QRCodeGenerator();
        var data = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
        var code = new Base64QRCode(data);
        return code.GetGraphic(20);
    }
}