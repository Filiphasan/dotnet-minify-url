namespace Web.Services.Interfaces;

public interface ICacheService
{
    Task<bool> PingAsync();
    Task SetAsync<TModel>(string key, TModel value, TimeSpan expiration)
        where TModel : class;
    Task SetAsync<TModel>(string key, TModel value, DateTimeOffset expiration)
        where TModel : class;
    Task<TModel?> GetAsync<TModel>(string key)
        where TModel : class;
    Task<bool> ExistsAsync(string key);
    Task RemoveAsync(string key);

    Task<long> AddListRightAsync<TModel>(string key, TModel value)
        where TModel : class;
    Task<TModel?> ListLeftPopAsync<TModel>(string key)
        where TModel : class;
}