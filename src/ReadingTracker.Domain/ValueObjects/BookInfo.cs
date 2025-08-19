namespace ReadingTracker.Domain.ValueObjects;

public record BookInfo
{
    public string Title { get; }
    public string Author { get; }
    public string? Isbn { get; }
    public string? Publisher { get; }
    public int? PublicationYear { get; }
    public int? TotalPages { get; }
    public string? Genre { get; }
    public string? Description { get; }
    public string? CoverImageUrl { get; }

    public BookInfo(
        string title, 
        string author, 
        string? isbn = null,
        string? publisher = null,
        int? publicationYear = null,
        int? totalPages = null,
        string? genre = null,
        string? description = null,
        string? coverImageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));
        
        if (string.IsNullOrWhiteSpace(author))
            throw new ArgumentException("Author cannot be empty", nameof(author));
        
        if (totalPages.HasValue && totalPages <= 0)
            throw new ArgumentException("Total pages must be positive", nameof(totalPages));

        if (publicationYear.HasValue && (publicationYear < 1000 || publicationYear > DateTime.UtcNow.Year + 10))
            throw new ArgumentException("Publication year must be reasonable", nameof(publicationYear));

        Title = title.Trim();
        Author = author.Trim();
        Isbn = isbn?.Trim();
        Publisher = publisher?.Trim();
        PublicationYear = publicationYear;
        TotalPages = totalPages;
        Genre = genre?.Trim();
        Description = description?.Trim();
        CoverImageUrl = coverImageUrl?.Trim();
    }

    public override string ToString() => $"{Title} by {Author}";
}
