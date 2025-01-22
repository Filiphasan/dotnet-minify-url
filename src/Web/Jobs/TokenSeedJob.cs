using MongoDB.Driver;
using Quartz;
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
        var cancellationToken = context.CancellationToken;
        _logger.LogInformation("Token seed job started");
        try
        {
            var unusedFilter = Builders<UrlToken>.Filter.Where(x => !x.IsUsed);
            var unsuedTokenCount = await dbContext.UrlTokens.CountDocumentsAsync(unusedFilter, cancellationToken: cancellationToken);
            _logger.LogInformation("Token seed job started with {Count} unused tokens", unsuedTokenCount);
            if (unsuedTokenCount < appSettingModel.UrlToken.PoolingSize)
            {
                var tokenBuilder = new TokenBuilder()
                    .WithEpoch(appSettingModel.UrlToken.EpochDate)
                    .WithAdditionalCharLength(3);

                var forCount = appSettingModel.UrlToken.PoolingSize - unsuedTokenCount + appSettingModel.UrlToken.ExtendSize;
                _logger.LogInformation("Token seed job started with {Count} unused tokens, Started Create New Token Count: {NewTokenCount}", unsuedTokenCount, forCount);

                var parallelOptions = new ParallelOptions
                {
                    CancellationToken = cancellationToken,
                    MaxDegreeOfParallelism = 10
                };
                await Parallel.ForAsync(0, forCount, parallelOptions, async (_, ct) =>
                {
                    try
                    {
                        var token = tokenBuilder.Build();
                        await dbContext.UrlTokens.InsertOneAsync(new UrlToken
                        {
                            Token = token,
                            IsUsed = false,
                            CreatedAt = DateTime.UtcNow,
                        }, null, ct);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred while generating token: {Message}", ex.Message);
                    }
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while executing the job: {Message}", ex.Message);
        }

        _logger.LogInformation("Token seed job completed");
    }
}