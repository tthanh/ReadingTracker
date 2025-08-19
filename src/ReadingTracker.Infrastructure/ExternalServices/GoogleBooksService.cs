using System.Text.Json;
using Microsoft.Extensions.Logging;
using ReadingTracker.Domain.ValueObjects;

namespace ReadingTracker.Infrastructure.ExternalServices;

public class GoogleBooksService : IBookSearchService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GoogleBooksService> _logger;
    private const string GoogleBooksApiUrl = "https://www.googleapis.com/books/v1/volumes";

    public GoogleBooksService(HttpClient httpClient, ILogger<GoogleBooksService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _httpClient.BaseAddress = new Uri(GoogleBooksApiUrl);
    }

    public async Task<IEnumerable<BookInfo>> SearchBooksAsync(string query, int maxResults = 10)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new List<BookInfo>();
        }

        try
        {
            var encodedQuery = Uri.EscapeDataString(query.Trim());
            var url = $"?q={encodedQuery}&maxResults={Math.Min(maxResults, 40)}";

            _logger.LogInformation("Searching Google Books API with query: {Query}", query);

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var googleResponse = JsonSerializer.Deserialize<GoogleBooksResponse>(json);

            if (googleResponse?.Items == null || googleResponse.Items.Length == 0)
            {
                _logger.LogInformation("No books found for query: {Query}", query);
                return new List<BookInfo>();
            }

            var books = googleResponse.Items
                .Where(item => item.VolumeInfo != null)
                .Select(ConvertToBookInfo)
                .Where(book => book != null)
                .Cast<BookInfo>()
                .ToList();

            _logger.LogInformation("Found {Count} books for query: {Query}", books.Count, query);
            return books;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while searching Google Books API");
            return new List<BookInfo>();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing Google Books API response");
            return new List<BookInfo>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while searching Google Books API");
            return new List<BookInfo>();
        }
    }

    public async Task<BookInfo?> GetBookByIsbnAsync(string isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn))
        {
            return null;
        }

        try
        {
            var cleanIsbn = isbn.Trim().Replace("-", "").Replace(" ", "");
            var url = $"?q=isbn:{cleanIsbn}";

            _logger.LogInformation("Searching Google Books API by ISBN: {ISBN}", cleanIsbn);

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var googleResponse = JsonSerializer.Deserialize<GoogleBooksResponse>(json);

            if (googleResponse?.Items == null || googleResponse.Items.Length == 0)
            {
                _logger.LogInformation("No book found for ISBN: {ISBN}", cleanIsbn);
                return null;
            }

            var book = ConvertToBookInfo(googleResponse.Items[0]);
            if (book != null)
            {
                _logger.LogInformation("Found book by ISBN: {ISBN} - {Title}", cleanIsbn, book.Title);
            }

            return book;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while searching Google Books API by ISBN");
            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing Google Books API response");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while searching Google Books API by ISBN");
            return null;
        }
    }

    private static BookInfo? ConvertToBookInfo(GoogleBookItem item)
    {
        var volumeInfo = item.VolumeInfo;
        if (volumeInfo == null || string.IsNullOrWhiteSpace(volumeInfo.Title))
        {
            return null;
        }

        try
        {
            var title = volumeInfo.Title.Trim();
            var author = string.Join(", ", volumeInfo.Authors ?? new[] { "Unknown Author" });
            
            // Extract ISBN
            string? isbn = null;
            if (volumeInfo.IndustryIdentifiers != null)
            {
                var isbnIdentifier = volumeInfo.IndustryIdentifiers
                    .FirstOrDefault(id => id.Type == "ISBN_13" || id.Type == "ISBN_10");
                isbn = isbnIdentifier?.Identifier;
            }

            // Parse publication year
            int? publicationYear = null;
            if (!string.IsNullOrWhiteSpace(volumeInfo.PublishedDate))
            {
                if (DateTime.TryParse(volumeInfo.PublishedDate, out var date))
                {
                    publicationYear = date.Year;
                }
                else if (int.TryParse(volumeInfo.PublishedDate.Substring(0, 4), out var year))
                {
                    publicationYear = year;
                }
            }

            // Get genre from categories
            string? genre = null;
            if (volumeInfo.Categories != null && volumeInfo.Categories.Length > 0)
            {
                genre = volumeInfo.Categories[0];
            }

            // Get cover image URL
            string? coverImageUrl = null;
            if (volumeInfo.ImageLinks != null)
            {
                coverImageUrl = volumeInfo.ImageLinks.Medium
                    ?? volumeInfo.ImageLinks.Small
                    ?? volumeInfo.ImageLinks.Thumbnail
                    ?? volumeInfo.ImageLinks.Large;
            }

            return new BookInfo(
                title: title,
                author: author,
                isbn: isbn,
                publisher: volumeInfo.Publisher?.Trim(),
                publicationYear: publicationYear,
                totalPages: volumeInfo.PageCount,
                genre: genre?.Trim(),
                description: volumeInfo.Description?.Trim(),
                coverImageUrl: coverImageUrl?.Trim()
            );
        }
        catch (Exception)
        {
            // Log the error but don't throw - just skip this book
            return null;
        }
    }
}
