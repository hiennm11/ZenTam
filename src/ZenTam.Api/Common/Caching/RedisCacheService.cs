using StackExchange.Redis;
using System.Text.Json;

namespace ZenTam.Api.Common.Caching;

public class RedisCacheService(IConnectionMultiplexer multiplexer, ILogger<RedisCacheService> logger) : ICacheService
{
    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        try
        {
            IDatabase db = multiplexer.GetDatabase();
            RedisValue raw = await db.StringGetAsync(key);
            if (raw.IsNullOrEmpty)
                return default(T?);
            return JsonSerializer.Deserialize<T>((string)raw!);
        }
        catch (RedisException ex)
        {
            logger.LogWarning("Redis GET failed for key {Key}: {Message}", key, ex.Message);
            return default(T?);
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default)
    {
        try
        {
            string json = JsonSerializer.Serialize(value);
            IDatabase db = multiplexer.GetDatabase();
            await db.StringSetAsync(key, json, ttl);
        }
        catch (RedisException ex)
        {
            logger.LogWarning("Redis SET failed for key {Key}: {Message}", key, ex.Message);
        }
    }
}
