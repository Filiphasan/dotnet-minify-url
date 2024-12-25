using MongoDB.Driver;
using Quartz;
using Web.Common.Constants;
using Web.Common.Models.Options;
using Web.Data;
using Web.Data.Entities;
using Web.Helpers;
using Web.Services.Interfaces;

namespace Web.Jobs;

public class TokenSeedJob(ILogger<TokenSeedJob> logger, MongoDbContext dbContext, ICacheService cacheService, AppSettingModel appSettingModel)
    : BaseJob<TokenSeedJob>(logger)
{
    private readonly ILogger<TokenSeedJob> _logger = logger;

    protected override async Task ExecuteAsync(IJobExecutionContext context)
    {
        _logger.LogInformation("Token seed job started");
        try
        {
            var unusedFilter = Builders<UrlToken>.Filter.Where(x => !x.IsUsed);
            var unsuedTokenCount = await dbContext.UrlTokens.CountDocumentsAsync(unusedFilter);
            _logger.LogInformation("Token seed job started with {Count} unused tokens", unsuedTokenCount);
            if (unsuedTokenCount < appSettingModel.UrlToken.PoolingSize)
            {
                var tokenBuilder = new TokenBuilder()
                    .WithEpoch(appSettingModel.UrlToken.EpochDate)
                    .WithAdditionalCharLength(3);

                var forCount = appSettingModel.UrlToken.PoolingSize - unsuedTokenCount + appSettingModel.UrlToken.ExtendSize;
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
                        await dbContext.UrlTokens.InsertOneAsync(urlToken);
                        await cacheService.AddListRightAsync(RedisConstant.Key.TokenSeedList, token);
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
        _logger.LogInformation("Token seed job completed");
    }
}