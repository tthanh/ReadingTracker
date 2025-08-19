namespace ReadingTracker.Domain.ValueObjects;

public record BookInfo
{
    public string Title { get; }
    public string Author { get; }
    public string? Genre { get; }
    public string? Isbn { get; }
    public int? TotalPages { get; }
    public string? Publisher { get; }
    public DateTime? PublishedDate { get; }

    public BookInfo(
        string title, 
        string author, 
        string? genre = null,
        string? isbn = null, 
        int? totalPages = null,
        string? publisher = null,
        DateTime? publishedDate = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));
        
        if (string.IsNullOrWhiteSpace(author))
            throw new ArgumentException("Author cannot be empty", nameof(author));
        
        if (totalPages.HasValue && totalPages <= 0)
            throw new ArgumentException("Total pages must be positive", nameof(totalPages));

        Title = title.Trim();
        Author = author.Trim();
        Genre = genre?.Trim();
        Isbn = isbn?.Trim();
        TotalPages = totalPages;
        Publisher = publisher?.Trim();
        PublishedDate = publishedDate;
    }

    public override string ToString() => $"{Title} by {Author}";
}
