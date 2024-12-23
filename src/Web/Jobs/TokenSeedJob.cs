using MongoDB.Driver;
using Quartz;
using Web.Constants;
using Web.Data;
using Web.Data.Entities;
using Web.Helpers;
using Web.Models.Options;
using Web.Services.Interfaces;

namespace Web.Jobs;

public class TokenSeedJob(ILogger<TokenSeedJob> logger, MongoDbContext dbContext, ICacheService cacheService, AppSettingModel appSettingModel)
    : BaseJob<TokenSeedJob>(logger)
{
    private readonly ILogger<TokenSeedJob> _logger = logger;

    protected override async Task ExecuteAsync(IJobExecutionContext context)
    {
        try
        {
            var unusedFilter = Builders<UrlToken>.Filter.Where(x => !x.IsUsed);
            var unsuedTokenCount = await dbContext.UrlTokens.CountDocumentsAsync(unusedFilter);
            if (unsuedTokenCount < appSettingModel.UrlToken.PoolingSize)
            {
                var tokenBuilder = new TokenBuilder()
                    .WithEpoch(appSettingModel.UrlToken.EpochDate)
                    .WithAdditionalCharLength(3);

                var forCount = appSettingModel.UrlToken.PoolingSize - unsuedTokenCount;
                for (int i = 0; i <= forCount; i++)
                {
                    try
                    {
                        string token;
                        do
                        {
                            token = tokenBuilder.Build();
                        } while (await dbContext.UrlTokens.Find(x => x.Token == token).AnyAsync());
                        var urlToken = new UrlToken
                        {
                            Token = token,
                            IsUsed = false,
                            CreatedAt = DateTime.UtcNow,
                        };

                        await cacheService.AddListRightAsync(RedisConstant.Key.TokenSeedList, token);
                        await dbContext.UrlTokens.InsertOneAsync(urlToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred while generating token: {Message}", ex.Message);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while executing the job: {Message}", ex.Message);
        }
    }
}