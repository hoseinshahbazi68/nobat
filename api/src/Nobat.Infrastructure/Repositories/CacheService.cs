using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Nobat.Infrastructure.Repositories;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);
    bool IsEnabled { get; }
}

public class CacheService : ICacheService
{
    private readonly IDistributedCache? _distributedCache;
    private readonly ILogger<CacheService> _logger;
    private readonly bool _isEnabled;

    public CacheService(IDistributedCache? distributedCache, ILogger<CacheService> logger, IConfiguration configuration)
    {
        _distributedCache = distributedCache;
        _logger = logger;
        _isEnabled = configuration.GetValue("Cache:Redis:Enabled", true);
    }

    public bool IsEnabled => _isEnabled;

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        if (!_isEnabled || _distributedCache == null)
        {
            _logger.LogDebug("Cache is disabled. Skipping cache get for key: {Key}", key);
            return null;
        }

        try
        {
            var cachedValue = await _distributedCache.GetStringAsync(key, cancellationToken);
            if (string.IsNullOrEmpty(cachedValue))
            {
                return null;
            }

            return JsonSerializer.Deserialize<T>(cachedValue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache value for key: {Key}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        if (!_isEnabled || _distributedCache == null)
        {
            _logger.LogDebug("Cache is disabled. Skipping cache set for key: {Key}", key);
            return;
        }

        try
        {
            var options = new DistributedCacheEntryOptions();
            if (expiration.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = expiration;
            }
            else
            {
                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            }

            var serializedValue = JsonSerializer.Serialize(value);
            await _distributedCache.SetStringAsync(key, serializedValue, options, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache value for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        if (!_isEnabled || _distributedCache == null)
        {
            return;
        }

        try
        {
            await _distributedCache.RemoveAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache value for key: {Key}", key);
        }
    }

    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        // Note: Redis doesn't support pattern deletion directly in IDistributedCache
        // This would require using StackExchange.Redis directly for pattern matching
        _logger.LogWarning("RemoveByPatternAsync is not fully implemented. Use RemoveAsync for specific keys.");
        return Task.CompletedTask;
    }
}

