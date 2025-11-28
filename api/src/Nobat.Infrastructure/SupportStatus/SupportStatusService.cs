using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Nobat.Infrastructure.SupportStatus;

/// <summary>
/// سرویس مدیریت وضعیت آنلاین پشتیبان‌ها با استفاده از Cache
/// </summary>
public class SupportStatusService : ISupportStatusService
{
    private readonly IDistributedCache? _cache;
    private readonly ILogger<SupportStatusService> _logger;
    private readonly TimeSpan _onlineTimeout = TimeSpan.FromMinutes(5); // اگر پشتیبان 5 دقیقه فعالیت نداشته باشد، آفلاین محسوب می‌شود
    private const string CacheKeyPrefix = "support_online_";

    public SupportStatusService(
        IDistributedCache? cache,
        ILogger<SupportStatusService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task SetSupportOnlineAsync(int supportUserId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_cache == null) return;

            var cacheKey = $"{CacheKeyPrefix}{supportUserId}";
            var timestamp = DateTime.UtcNow.ToString("O");

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _onlineTimeout
            };

            await _cache.SetStringAsync(cacheKey, timestamp, options, cancellationToken);
            _logger.LogDebug("Support user {SupportUserId} marked as online", supportUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting support online status for user {SupportUserId}", supportUserId);
        }
    }

    public async Task SetSupportOfflineAsync(int supportUserId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_cache == null) return;

            var cacheKey = $"{CacheKeyPrefix}{supportUserId}";
            await _cache.RemoveAsync(cacheKey, cancellationToken);
            _logger.LogDebug("Support user {SupportUserId} marked as offline", supportUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting support offline status for user {SupportUserId}", supportUserId);
        }
    }

    public async Task<bool> IsAnySupportOnlineAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_cache == null) return false;

            // بررسی پشتیبان‌های آنلاین (از 1 تا 100 بررسی می‌کنیم)
            for (int i = 1; i <= 100; i++)
            {
                var cacheKey = $"{CacheKeyPrefix}{i}";
                var value = await _cache.GetStringAsync(cacheKey, cancellationToken);
                if (!string.IsNullOrEmpty(value))
                {
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if any support is online");
            return false;
        }
    }

    public async Task<int> GetOnlineSupportCountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_cache == null) return 0;

            int count = 0;
            // بررسی پشتیبان‌های آنلاین (از 1 تا 100 بررسی می‌کنیم)
            for (int i = 1; i <= 100; i++)
            {
                var cacheKey = $"{CacheKeyPrefix}{i}";
                var value = await _cache.GetStringAsync(cacheKey, cancellationToken);
                if (!string.IsNullOrEmpty(value))
                {
                    count++;
                }
            }

            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting online support count");
            return 0;
        }
    }
}
