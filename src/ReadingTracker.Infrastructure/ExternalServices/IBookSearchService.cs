using System.Text.Json.Serialization;
using ReadingTracker.Domain.ValueObjects;

namespace ReadingTracker.Infrastructure.ExternalServices;

// Interface for book search service
public interface IBookSearchService
{
    Task<IEnumerable<BookInfo>> SearchBooksAsync(string query, int maxResults = 10);
    Task<BookInfo?> GetBookByIsbnAsync(string isbn);
}

// Google Books API models
public class GoogleBooksResponse
{
    [JsonPropertyName("items")]
    public GoogleBookItem[]? Items { get; set; }

    [JsonPropertyName("totalItems")]
    public int TotalItems { get; set; }
}

public class GoogleBookItem
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("volumeInfo")]
    public GoogleVolumeInfo? VolumeInfo { get; set; }
}

public class GoogleVolumeInfo
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("authors")]
    public string[]? Authors { get; set; }

    [JsonPropertyName("publisher")]
    public string? Publisher { get; set; }

    [JsonPropertyName("publishedDate")]
    public string? PublishedDate { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("industryIdentifiers")]
    public GoogleIndustryIdentifier[]? IndustryIdentifiers { get; set; }

    [JsonPropertyName("pageCount")]
    public int? PageCount { get; set; }

    [JsonPropertyName("categories")]
    public string[]? Categories { get; set; }

    [JsonPropertyName("imageLinks")]
    public GoogleImageLinks? ImageLinks { get; set; }
}

public class GoogleIndustryIdentifier
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("identifier")]
    public string? Identifier { get; set; }
}

public class GoogleImageLinks
{
    [JsonPropertyName("thumbnail")]
    public string? Thumbnail { get; set; }

    [JsonPropertyName("small")]
    public string? Small { get; set; }

    [JsonPropertyName("medium")]
    public string? Medium { get; set; }

    [JsonPropertyName("large")]
    public string? Large { get; set; }
}
