using MediatR;
using MongoDB.Driver;
using Web.Common.Constants;
using Web.Common.Models.Endpoints;
using Web.Data;
using Web.Services.Interfaces;

namespace Web.UseCases.UrlShorten.GetShortenedUrl;

public class GetShortenedUrlHandler(ICacheService cacheService, MongoDbContext dbContext) : IRequestHandler<GetShortenedUrlQuery, Result<GetShortenedUrlResponse>>
{
    public async Task<Result<GetShortenedUrlResponse>> Handle(GetShortenedUrlQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return Result<GetShortenedUrlResponse>.Invalid("Token is required.");
        }

        var cacheKey = string.Format(RedisConstant.Key.ShortUrl, request.Token);
        var url = await cacheService.GetAsync<string>(cacheKey, cancellationToken);
        if (url is null)
        {
            var shortUrl = await dbContext.UrlShortens.Find(x => x.Token == request.Token).FirstOrDefaultAsync(cancellationToken);
            if (shortUrl is null)
            {
                return Result<GetShortenedUrlResponse>.Error(404, "Shortened URL not found.");
            }

            url = shortUrl.Url;
            await cacheService.SetAsync(cacheKey, url, shortUrl.ExpiredAt, cancellationToken);
        }

        return Result<GetShortenedUrlResponse>.Success(new GetShortenedUrlResponse { LongUrl = url });
    }
}