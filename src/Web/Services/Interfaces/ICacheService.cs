namespace Web.Services.Interfaces;

public interface ICacheService
{
    Task<bool> PingAsync(CancellationToken cancellationToken = default);
    Task SetAsync<TModel>(string key, TModel value, TimeSpan expiration, CancellationToken cancellationToken = default)
        where TModel : class;
    Task SetAsync<TModel>(string key, TModel value, DateTimeOffset expiration, CancellationToken cancellationToken = default)
        where TModel : class;
    Task<TModel?> GetAsync<TModel>(string key, CancellationToken cancellationToken = default)
        where TModel : class;
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task<long> AddListRightAsync<TModel>(string key, TModel value, CancellationToken cancellationToken = default)
        where TModel : class;
    Task<long> AddListRightBulkAsync<TModel>(string key, TModel[] values, CancellationToken cancellationToken = default)
        where TModel : class;
    Task<TModel?> ListLeftPopAsync<TModel>(string key, CancellationToken cancellationToken = default)
        where TModel : class;
}