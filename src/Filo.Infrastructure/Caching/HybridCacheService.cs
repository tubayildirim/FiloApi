using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Filo.Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;

namespace Filo.Infrastructure.Caching;

public class HybridCacheService : ICacheService
{
    private readonly HybridCache _hybridCache;
    private readonly IDistributedCache _distributedCache;

    public HybridCacheService(HybridCache hybridCache, IDistributedCache distributedCache)
    {
        _hybridCache = hybridCache;
        _distributedCache = distributedCache;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var cachedString = await _distributedCache.GetStringAsync(key);
        if (string.IsNullOrEmpty(cachedString))
            return default;

        return JsonSerializer.Deserialize<T>(cachedString);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var options = new HybridCacheEntryOptions
        {
            Expiration = expiration
        };

        await _hybridCache.SetAsync(key, value, options);
    }

    public async Task RemoveAsync(string key)
    {
        await _hybridCache.RemoveAsync(key);
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan? expiration = null)
    {
        var options = new HybridCacheEntryOptions
        {
            Expiration = expiration
        };

        return await _hybridCache.GetOrCreateAsync(
            key,
            async token => await factory(token),
            options,
            cancellationToken: default);
    }
}
