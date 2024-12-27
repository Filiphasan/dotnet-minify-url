using MediatR;
using MongoDB.Driver;
using Web.Common.Constants;
using Web.Common.Models.Endpoints;
using Web.Common.Models.Options;
using Web.Data;
using Web.Helpers;
using Web.Services.Interfaces;
using Web.UseCases.UrlToken.SetTokenUsed;

namespace Web.UseCases.UrlToken.GetUnusedToken;

public class GetUnusedTokenHandler(ISender sender, ICacheService cacheService, MongoDbContext dbContext, AppSettingModel appSettingModel) : IRequestHandler<GetUnusedTokenQuery, Result<GetUnusedTokenResponse>>
{
    public async Task<Result<GetUnusedTokenResponse>> Handle(GetUnusedTokenQuery request, CancellationToken cancellationToken)
    {
        bool sendTokenUsed = true;
        var token = await cacheService.ListLeftPopAsync<string>(RedisConstant.Key.TokenSeedList, cancellationToken);
        if (string.IsNullOrEmpty(token))
        {
            var tokenBuilder = new TokenBuilder()
                .WithEpoch(appSettingModel.UrlToken.EpochDate)
                .WithAdditionalCharLength(3);
            do
            {
                token = tokenBuilder.Build();
            } while (await dbContext.UrlTokens.Find(x => x.Token == token).AnyAsync(cancellationToken));

            var now = DateTime.UtcNow;
            await dbContext.UrlTokens.InsertOneAsync(new Data.Entities.UrlToken
            {
                Token = token,
                IsUsed = true,
                CreatedAt = now,
                UsedAt = now,
            }, cancellationToken: cancellationToken);
            sendTokenUsed = false;
        }

        if (sendTokenUsed)
        {
            await sender.Send(new SetTokenUsedCommand { Token = token }, cancellationToken);
        }
        return Result<GetUnusedTokenResponse>.Success(new GetUnusedTokenResponse { Token = token });
    }
}