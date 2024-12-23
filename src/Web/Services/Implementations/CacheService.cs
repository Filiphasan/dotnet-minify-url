using System.Text.Json;
using StackExchange.Redis;
using Web.Models.Options;
using Web.Services.Interfaces;

namespace Web.Services.Implementations;

public class CacheService(IConnectionMultiplexer connectionMultiplexer, AppSettingModel appSettingModel) : ICacheService
{
    private readonly IDatabase _database = connectionMultiplexer.GetDatabase(appSettingModel.Redis.Database);

    public async Task<bool> PingAsync()
    {
        var ts = await _database.PingAsync();
        return ts.TotalMilliseconds > 0;
    }

    public async Task SetAsync<TModel>(string key, TModel value, TimeSpan expiration) where TModel : class
    {
        var redisValue = JsonSerializer.Serialize(value);
        await _database.StringSetAsync(key, redisValue, expiration);
    }

    public async Task SetAsync<TModel>(string key, TModel value, DateTimeOffset expiration) where TModel : class
    {
        var redisValue = JsonSerializer.Serialize(value);
        await _database.StringSetAsync(key, redisValue, TimeSpan.FromTicks(expiration.Ticks));
    }

    public async Task<TModel?> GetAsync<TModel>(string key) where TModel : class
    {
        var redisValue = await _database.StringGetAsync(key);
        return redisValue.HasValue
            ? JsonSerializer.Deserialize<TModel>(redisValue.ToString())
            : null;
    }

    public async Task<bool> ExistsAsync(string key)
    {
        return await _database.KeyExistsAsync(key);
    }

    public async Task RemoveAsync(string key)
    {
        await _database.KeyDeleteAsync(key);
    }

    public async Task<long> AddListRightAsync<TModel>(string key, TModel value) where TModel : class
    {
        return await _database.ListRightPushAsync(key, JsonSerializer.Serialize(value));
    }

    public async Task<TModel?> ListLeftPopAsync<TModel>(string key) where TModel : class
    {
        var redisValue = await _database.ListLeftPopAsync(key);
        return redisValue.HasValue ? JsonSerializer.Deserialize<TModel>(redisValue.ToString()) : null;
    }
}