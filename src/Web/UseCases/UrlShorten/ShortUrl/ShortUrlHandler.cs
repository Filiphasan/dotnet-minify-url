using MediatR;
using MongoDB.Driver;
using QRCoder;
using Web.Common.Constants;
using Web.Common.Models.Endpoints;
using Web.Common.Models.Options;
using Web.Data;
using Web.Services.Interfaces;
using Web.UseCases.UrlToken.GetUnusedToken;

namespace Web.UseCases.UrlShorten.ShortUrl;

public class ShortUrlHandler(ISender sender, MongoDbContext dbContext, ICacheService cacheService, AppSettingModel appSettingModel)
    : IRequestHandler<ShortUrlCommand, Result<ShortUrlResponse>>
{
    public async Task<Result<ShortUrlResponse>> Handle(ShortUrlCommand request, CancellationToken cancellationToken)
    {
        var response = new ShortUrlResponse();
        string token;
        var urlExist = await dbContext.UrlShortens.Find(x => x.Url == request.Url && x.ExpiredAt > DateTime.UtcNow).FirstOrDefaultAsync(cancellationToken);
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
        response.ShortenedUrl = $"{appSettingModel.Server.Url}/{token}";
        response.QrCode = GetQrBase64(request, response.ShortenedUrl);
        var expireDay = request.ExpireDay ?? appSettingModel.UrlToken.ExpirationDays;
        var urlShorten = new Data.Entities.UrlShorten
        {
            Url = request.Url!,
            Token = token,
            CreatedAt = DateTime.UtcNow,
            ExpiredAt = DateTime.UtcNow.AddDays(expireDay)
        };
        await dbContext.UrlShortens.InsertOneAsync(urlShorten, null, cancellationToken);
        await cacheService.SetAsync(string.Format(RedisConstant.Key.ShortUrl, token), request.Url!, TimeSpan.FromDays(expireDay), cancellationToken);

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