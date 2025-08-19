using ReadingTracker.Domain.Aggregates;
using ReadingTracker.Domain.Repositories;
using ReadingTracker.Domain.ValueObjects;
using ReadingTracker.Infrastructure.Services;

namespace ReadingTracker.Infrastructure.Persistence.Repositories;

public class CachedUserBookRepository : IUserBookRepository
{
    private readonly IUserBookRepository _repository;
    private readonly ICacheService _cache;

    public CachedUserBookRepository(IUserBookRepository repository, ICacheService cache)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task<UserBook?> GetByIdAsync(Guid userBookId)
    {
        var cacheKey = CacheKeys.UserBook(userBookId);
        var cached = await _cache.GetAsync<UserBook>(cacheKey);
        
        if (cached != null)
            return cached;

        var userBook = await _repository.GetByIdAsync(userBookId);
        
        if (userBook != null)
        {
            await _cache.SetAsync(cacheKey, userBook);
        }

        return userBook;
    }

    public async Task<UserBook?> GetByUserAndBookAsync(Guid userId, string bookId)
    {
        // For this method, we don't cache because it's a complex key
        return await _repository.GetByUserAndBookAsync(userId, bookId);
    }

    public async Task<IEnumerable<UserBook>> GetByUserIdAsync(Guid userId)
    {
        var cacheKey = CacheKeys.UserBooks(userId);
        var cached = await _cache.GetAsync<List<UserBook>>(cacheKey);
        
        if (cached != null)
            return cached;

        var userBooks = await _repository.GetByUserIdAsync(userId);
        var userBooksList = userBooks.ToList();
        
        await _cache.SetAsync(cacheKey, userBooksList);

        return userBooksList;
    }

    public async Task AddAsync(UserBook userBook)
    {
        await _repository.AddAsync(userBook);
        
        // Invalidate relevant cache entries
        await InvalidateUserCaches(userBook.UserId);
        await _cache.SetAsync(CacheKeys.UserBook(userBook.UserBookId), userBook);
    }

    public async Task UpdateAsync(UserBook userBook)
    {
        await _repository.UpdateAsync(userBook);
        
        // Invalidate relevant cache entries
        await InvalidateUserCaches(userBook.UserId);
        await _cache.RemoveAsync(CacheKeys.UserBook(userBook.UserBookId));
    }

    public async Task DeleteAsync(Guid userBookId)
    {
        // Get the user book first to know which user's cache to invalidate
        var userBook = await _repository.GetByIdAsync(userBookId);
        
        await _repository.DeleteAsync(userBookId);
        
        if (userBook != null)
        {
            await InvalidateUserCaches(userBook.UserId);
        }
        
        await _cache.RemoveAsync(CacheKeys.UserBook(userBookId));
    }

    public async Task<IEnumerable<UserBook>> FindByStatusAsync(Guid userId, ReadingStatus status)
    {
        var cacheKey = CacheKeys.UserBooksByStatus(userId, status);
        var cached = await _cache.GetAsync<List<UserBook>>(cacheKey);
        
        if (cached != null)
            return cached;

        var userBooks = await _repository.FindByStatusAsync(userId, status);
        var userBooksList = userBooks.ToList();
        
        await _cache.SetAsync(cacheKey, userBooksList);

        return userBooksList;
    }

    public async Task<IEnumerable<UserBook>> FindCurrentlyReadingAsync(Guid userId)
    {
        return await FindByStatusAsync(userId, ReadingStatus.Reading);
    }

    public async Task<IEnumerable<UserBook>> FindRecentlyFinishedAsync(Guid userId, int days = 30)
    {
        // Don't cache this as it's time-sensitive
        return await _repository.FindRecentlyFinishedAsync(userId, days);
    }

    public async Task<IEnumerable<UserBook>> FindByRatingAsync(Guid userId, int rating)
    {
        // Don't cache this as it's less frequently accessed
        return await _repository.FindByRatingAsync(userId, rating);
    }

    public async Task<IEnumerable<UserBook>> SearchAsync(Guid userId, string searchTerm)
    {
        // Don't cache search results as they can be very dynamic
        return await _repository.SearchAsync(userId, searchTerm);
    }

    public async Task<IEnumerable<UserBook>> FindByAuthorAsync(Guid userId, string author)
    {
        // Don't cache this as it's less frequently accessed
        return await _repository.FindByAuthorAsync(userId, author);
    }

    public async Task<IEnumerable<UserBook>> FindByDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate)
    {
        // Don't cache this as it's very specific and less frequently accessed
        return await _repository.FindByDateRangeAsync(userId, startDate, endDate);
    }

    public async Task<int> GetTotalBooksCountAsync(Guid userId)
    {
        // Don't cache counts as they change frequently
        return await _repository.GetTotalBooksCountAsync(userId);
    }

    public async Task<int> GetBooksCountByStatusAsync(Guid userId, ReadingStatus status)
    {
        // Don't cache counts as they change frequently
        return await _repository.GetBooksCountByStatusAsync(userId, status);
    }

    public async Task<bool> HasUserReadBookAsync(Guid userId, string bookId)
    {
        // Don't cache this simple check
        return await _repository.HasUserReadBookAsync(userId, bookId);
    }

    public async Task<(IEnumerable<UserBook> Books, int TotalCount)> GetPagedAsync(
        Guid userId, 
        int pageNumber, 
        int pageSize,
        ReadingStatus? statusFilter = null,
        string? searchTerm = null)
    {
        // Don't cache paged results as they are very specific
        return await _repository.GetPagedAsync(userId, pageNumber, pageSize, statusFilter, searchTerm);
    }

    private async Task InvalidateUserCaches(Guid userId)
    {
        await _cache.RemoveAsync(CacheKeys.UserBooks(userId));
        await _cache.RemoveAsync(CacheKeys.UserBooksStats(userId));
        
        // Invalidate status-specific caches
        foreach (ReadingStatus status in Enum.GetValues<ReadingStatus>())
        {
            await _cache.RemoveAsync(CacheKeys.UserBooksByStatus(userId, status));
        }
    }
}
