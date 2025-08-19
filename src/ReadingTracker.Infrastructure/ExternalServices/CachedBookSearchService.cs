using ReadingTracker.Domain.ValueObjects;
using ReadingTracker.Infrastructure.Services;

namespace ReadingTracker.Infrastructure.ExternalServices;

public class CachedBookSearchService : IBookSearchService
{
    private readonly IBookSearchService _bookSearchService;
    private readonly ICacheService _cache;

    public CachedBookSearchService(IBookSearchService bookSearchService, ICacheService cache)
    {
        _bookSearchService = bookSearchService ?? throw new ArgumentNullException(nameof(bookSearchService));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task<IEnumerable<BookInfo>> SearchBooksAsync(string query, int maxResults = 10)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<BookInfo>();

        var cacheKey = CacheKeys.BookSearch($"{query}_{maxResults}");
        var cached = await _cache.GetAsync<List<BookInfo>>(cacheKey);
        
        if (cached != null)
            return cached;

        var books = await _bookSearchService.SearchBooksAsync(query, maxResults);
        var booksList = books.ToList();
        
        // Cache book search results for longer since external API results don't change often
        await _cache.SetAsync(cacheKey, booksList, TimeSpan.FromHours(2));

        return booksList;
    }

    public async Task<BookInfo?> GetBookByIsbnAsync(string isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn))
            return null;

        var cacheKey = CacheKeys.BookByIsbn(isbn);
        var cached = await _cache.GetAsync<BookInfo>(cacheKey);
        
        if (cached != null)
            return cached;

        var book = await _bookSearchService.GetBookByIsbnAsync(isbn);
        
        if (book != null)
        {
            // Cache ISBN lookups for a long time since they rarely change
            await _cache.SetAsync(cacheKey, book, TimeSpan.FromHours(24));
        }

        return book;
    }
}
