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
        var cancellationToken = context.CancellationToken;
        _logger.LogInformation("Token seed job started");

        try
        {
            int unusedTokenCount = await GetUnusedTokenCountAsync(cancellationToken);
            _logger.LogInformation("Token seed job found {Count} unused tokens", unusedTokenCount);

            if (ShouldGenerateMoreTokens(unusedTokenCount))
            {
                int tokensToGenerate = CalculateTokensToGenerate(unusedTokenCount);
                _logger.LogInformation("Generating {NewTokenCount} new tokens", tokensToGenerate);

                await GenerateTokensInParallelAsync(tokensToGenerate, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while executing the token seed job: {Message}", ex.Message);
        }

        _logger.LogInformation("Token seed job completed");
    }

    private async Task<int> GetUnusedTokenCountAsync(CancellationToken cancellationToken)
    {
        var unusedFilter = Builders<UrlToken>.Filter.Where(x => !x.IsUsed);
        return (int)await dbContext.UrlTokens.CountDocumentsAsync(unusedFilter, cancellationToken: cancellationToken);
    }

    private bool ShouldGenerateMoreTokens(int unusedTokenCount)
    {
        return unusedTokenCount < appSettingModel.UrlToken.PoolingSize;
    }

    private int CalculateTokensToGenerate(int unusedTokenCount)
    {
        return appSettingModel.UrlToken.PoolingSize - unusedTokenCount + appSettingModel.UrlToken.ExtendSize;
    }

    private async Task GenerateTokensInParallelAsync(int count, CancellationToken cancellationToken)
    {
        var tokenBuilder = new TokenBuilder()
            .WithEpoch(appSettingModel.UrlToken.EpochDate)
            .WithAdditionalCharLength(3);

        var parallelOptions = new ParallelOptions
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = 10
        };

        await Parallel.ForAsync(0, count, parallelOptions, async (_, ct) =>
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
                await cacheService.AddListRightAsync(RedisConstant.Key.TokenSeedList, token, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating token: {Message}", ex.Message);
            }
        });
    }
}