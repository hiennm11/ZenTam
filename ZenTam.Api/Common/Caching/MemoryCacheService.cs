using Microsoft.Extensions.Caching.Memory;

namespace ZenTam.Api.Common.Caching;

public class MemoryCacheService(IMemoryCache cache) : ICacheService
{
    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        cache.TryGetValue(key, out T? value);
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default)
    {
        cache.Set(key, value, ttl);
        return Task.CompletedTask;
    }
}