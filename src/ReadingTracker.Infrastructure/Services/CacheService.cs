using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ReadingTracker.Domain.Aggregates;
using ReadingTracker.Domain.ValueObjects;

namespace ReadingTracker.Infrastructure.Services;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;
    Task RemoveAsync(string key);
    Task RemovePatternAsync(string pattern);
}

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<MemoryCacheService> _logger;

    public MemoryCacheService(IMemoryCache cache, ILogger<MemoryCacheService> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<T?> GetAsync<T>(string key) where T : class
    {
        if (string.IsNullOrWhiteSpace(key))
            return Task.FromResult<T?>(null);

        try
        {
            var value = _cache.Get<T>(key);
            _logger.LogDebug("Cache {Action} for key: {Key}", value != null ? "hit" : "miss", key);
            return Task.FromResult(value);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting cache value for key: {Key}", key);
            return Task.FromResult<T?>(null);
        }
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        if (string.IsNullOrWhiteSpace(key) || value == null)
            return Task.CompletedTask;

        try
        {
            var options = new MemoryCacheEntryOptions();
            
            if (expiration.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = expiration.Value;
            }
            else
            {
                // Default expiration times based on data type
                if (typeof(T) == typeof(UserBook) || typeof(T) == typeof(List<UserBook>))
                {
                    options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                }
                else if (typeof(T) == typeof(BookInfo) || typeof(T) == typeof(List<BookInfo>))
                {
                    options.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6);
                }
                else
                {
                    options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
                }
            }

            // Set sliding expiration for frequently accessed data
            options.SlidingExpiration = TimeSpan.FromMinutes(5);

            _cache.Set(key, value, options);
            _logger.LogDebug("Cache set for key: {Key} with expiration: {Expiration}", key, options.AbsoluteExpirationRelativeToNow);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error setting cache value for key: {Key}", key);
        }

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return Task.CompletedTask;

        try
        {
            _cache.Remove(key);
            _logger.LogDebug("Cache removed for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error removing cache value for key: {Key}", key);
        }

        return Task.CompletedTask;
    }

    public Task RemovePatternAsync(string pattern)
    {
        // MemoryCache doesn't support pattern-based removal
        // This would need a more sophisticated implementation with a distributed cache like Redis
        _logger.LogWarning("Pattern-based cache removal is not supported with MemoryCache. Pattern: {Pattern}", pattern);
        return Task.CompletedTask;
    }
}

// Extension methods for common cache keys
public static class CacheKeys
{
    public static string UserBooks(Guid userId) => $"user_books_{userId}";
    public static string UserBook(Guid userBookId) => $"user_book_{userBookId}";
    public static string UserBooksByStatus(Guid userId, ReadingStatus status) => $"user_books_{userId}_{status}";
    public static string BookSearch(string query) => $"book_search_{query.ToLowerInvariant().Replace(" ", "_")}";
    public static string BookByIsbn(string isbn) => $"book_isbn_{isbn}";
    public static string UserBooksStats(Guid userId) => $"user_books_stats_{userId}";
}
