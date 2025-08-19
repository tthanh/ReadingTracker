using System.ComponentModel.DataAnnotations;

namespace ReadingTracker.Api.Models;

public class BookInfoDto
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? Isbn { get; set; }
    public string? Publisher { get; set; }
    public int? PublicationYear { get; set; }
    public int? TotalPages { get; set; }
    public string? Genre { get; set; }
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
}

public class UserBookDto
{
    public Guid UserBookId { get; set; }
    public string BookId { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public BookInfoDto BookInfo { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public ProgressDto CurrentProgress { get; set; } = new();
    public DateTime AddedDate { get; set; }
    public DateTime? StartedDate { get; set; }
    public DateTime? FinishedDate { get; set; }
    public string? PersonalNotes { get; set; }
    public int? PersonalRating { get; set; }
    public List<ReadingSessionDto> ReadingSessions { get; set; } = new();
    
    // Calculated properties
    public int TotalPagesRead { get; set; }
    public TimeSpan TotalReadingTime { get; set; }
    public int TotalReadingSessions { get; set; }
    public DateTime? LastReadingSessionDate { get; set; }
}

public class ProgressDto
{
    public int PageNumber { get; set; }
    public int? TotalPages { get; set; }
    public decimal PercentageComplete { get; set; }
    public bool IsComplete { get; set; }
}

public class ReadingSessionDto
{
    public Guid SessionId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndTime { get; set; }
    public int StartPage { get; set; }
    public int EndPage { get; set; }
    public string? SessionNotes { get; set; }
    public int PagesRead { get; set; }
    public TimeSpan? Duration { get; set; }
}

// Request DTOs
public class AddBookToLibraryRequest
{
    [Required]
    public string BookId { get; set; } = string.Empty;
    
    [Required]
    public BookInfoDto BookInfo { get; set; } = new();
    
    public string? PersonalNotes { get; set; }
}

public class UpdateBookStatusRequest
{
    [Required]
    public string Status { get; set; } = string.Empty;
    
    public DateTime? Date { get; set; }
}

public class LogReadingSessionRequest
{
    [Required]
    public DateTime SessionDate { get; set; }
    
    [Required]
    [Range(1, int.MaxValue)]
    public int StartPage { get; set; }
    
    [Required]
    [Range(1, int.MaxValue)]
    public int EndPage { get; set; }
    
    public DateTime? EndTime { get; set; }
    public string? SessionNotes { get; set; }
}

public class UpdateReadingProgressRequest
{
    [Required]
    [Range(0, int.MaxValue)]
    public int PageNumber { get; set; }
}

public class UpdatePersonalNotesRequest
{
    public string? PersonalNotes { get; set; }
}

public class UpdateBookRatingRequest
{
    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }
}

public class BookSearchRequest
{
    [Required]
    public string Query { get; set; } = string.Empty;
    
    [Range(1, 40)]
    public int MaxResults { get; set; } = 10;
}

// Response DTOs
public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;
}

public class ReadingStatisticsDto
{
    public int TotalBooks { get; set; }
    public int BooksRead { get; set; }
    public int BooksCurrentlyReading { get; set; }
    public int BooksToRead { get; set; }
    public int BooksOnHold { get; set; }
    public int BooksDropped { get; set; }
    public int TotalPagesRead { get; set; }
    public TimeSpan TotalReadingTime { get; set; }
    public double AverageRating { get; set; }
    public int TotalReadingSessions { get; set; }
    public DateTime? LastReadingSession { get; set; }
    public List<UserBookDto> RecentlyFinished { get; set; } = new();
    public List<UserBookDto> CurrentlyReading { get; set; } = new();
}
