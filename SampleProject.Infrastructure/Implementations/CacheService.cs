using Common.Shared.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.Distributed;

namespace SampleProject.Infrastructure.Implementations
{
    /// <summary>
    /// Cache service implementation
    /// </summary>
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<CacheService> _logger;

        public CacheService(IMemoryCache memoryCache, IDistributedCache distributedCache, ILogger<CacheService> logger)
        {
            _memoryCache = memoryCache;
            _distributedCache = distributedCache;
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                if (_memoryCache.TryGetValue(key, out T? memoryValue))
                {
                    return memoryValue;
                }

                var distributedValue = await _distributedCache.GetStringAsync(key, cancellationToken);
                if (distributedValue != null)
                {
                    var value = System.Text.Json.JsonSerializer.Deserialize<T>(distributedValue);
                    _memoryCache.Set(key, value, TimeSpan.FromMinutes(5));
                    return value;
                }

                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cache value for key: {Key}", key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var jsonValue = System.Text.Json.JsonSerializer.Serialize(value);
                var cacheExpiration = expiration ?? TimeSpan.FromMinutes(30);

                _memoryCache.Set(key, value, cacheExpiration);
                await _distributedCache.SetStringAsync(key, jsonValue, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = cacheExpiration
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache value for key: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                _memoryCache.Remove(key);
                await _distributedCache.RemoveAsync(key, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache value for key: {Key}", key);
            }
        }

        public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
        {
            _logger.LogWarning("RemoveByPatternAsync is not fully implemented for pattern: {Pattern}", pattern);
            await Task.CompletedTask;
        }
    }
}
