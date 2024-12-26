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

                var list = new HashSet<string>();
                var forCount = appSettingModel.UrlToken.PoolingSize - unsuedTokenCount + appSettingModel.UrlToken.ExtendSize;
                _logger.LogInformation("Token seed job started with {Count} unused tokens, Started Create New Token Count: {NewTokenCount}", unsuedTokenCount, forCount);

                for (int i = 0; i <= forCount; i++)
                {
                    try
                    {
                        string token;
                        do
                        {
                            token = tokenBuilder.Build();
                        } while (await dbContext.UrlTokens.Find(x => x.Token == token).AnyAsync());

                        list.Add(token);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred while generating token: {Message}", ex.Message);
                    }
                }

                foreach (var chunkList in list.Chunk(10_000))
                {
                    try
                    {
                        var urlTokens = chunkList.Select(token => new UrlToken
                        {
                            Token = token,
                            IsUsed = false,
                            CreatedAt = DateTime.UtcNow,
                        });
                        await dbContext.UrlTokens.InsertManyAsync(urlTokens);
                        await cacheService.AddListRightBulkAsync(RedisConstant.Key.TokenSeedList, chunkList.ToArray());
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred while bulk insert token: {Message}", ex.Message);
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