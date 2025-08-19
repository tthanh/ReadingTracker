using ReadingTracker.Api.Models;
using ReadingTracker.Domain.Aggregates;
using ReadingTracker.Domain.Entities;
using ReadingTracker.Domain.ValueObjects;

namespace ReadingTracker.Api.Extensions;

public static class MappingExtensions
{
    // BookInfo mappings
    public static BookInfoDto ToDto(this BookInfo bookInfo)
    {
        return new BookInfoDto
        {
            Title = bookInfo.Title,
            Author = bookInfo.Author,
            Isbn = bookInfo.Isbn,
            Publisher = bookInfo.Publisher,
            PublicationYear = bookInfo.PublicationYear,
            TotalPages = bookInfo.TotalPages,
            Genre = bookInfo.Genre,
            Description = bookInfo.Description,
            CoverImageUrl = bookInfo.CoverImageUrl
        };
    }

    public static BookInfo ToDomain(this BookInfoDto dto)
    {
        return new BookInfo(
            title: dto.Title,
            author: dto.Author,
            isbn: dto.Isbn,
            publisher: dto.Publisher,
            publicationYear: dto.PublicationYear,
            totalPages: dto.TotalPages,
            genre: dto.Genre,
            description: dto.Description,
            coverImageUrl: dto.CoverImageUrl
        );
    }

    // Progress mappings
    public static ProgressDto ToDto(this Progress progress)
    {
        return new ProgressDto
        {
            PageNumber = progress.PageNumber,
            TotalPages = progress.TotalPages,
            PercentageComplete = progress.Percentage ?? 0,
            IsComplete = progress.IsComplete
        };
    }

    // ReadingSession mappings
    public static ReadingSessionDto ToDto(this ReadingSession session)
    {
        return new ReadingSessionDto
        {
            SessionId = session.SessionId,
            StartDate = session.StartDate,
            EndTime = session.EndDate,
            StartPage = session.StartPage,
            EndPage = session.EndPage,
            SessionNotes = session.Notes,
            PagesRead = session.PagesRead,
            Duration = session.Duration
        };
    }

    // UserBook mappings
    public static UserBookDto ToDto(this UserBook userBook)
    {
        return new UserBookDto
        {
            UserBookId = userBook.UserBookId,
            BookId = userBook.BookId,
            UserId = userBook.UserId,
            BookInfo = userBook.BookInfo.ToDto(),
            Status = userBook.Status.ToString(),
            CurrentProgress = userBook.CurrentProgress.ToDto(),
            AddedDate = userBook.AddedDate,
            StartedDate = userBook.StartedDate,
            FinishedDate = userBook.FinishedDate,
            PersonalNotes = userBook.PersonalNotes,
            PersonalRating = userBook.PersonalRating,
            ReadingSessions = userBook.ReadingSessions.Select(s => s.ToDto()).ToList(),
            TotalPagesRead = userBook.TotalPagesRead,
            TotalReadingTime = userBook.TotalReadingTime,
            TotalReadingSessions = userBook.TotalReadingSessions,
            LastReadingSessionDate = userBook.LastReadingSessionDate
        };
    }

    // Collection mappings
    public static List<UserBookDto> ToDto(this IEnumerable<UserBook> userBooks)
    {
        return userBooks.Select(ub => ub.ToDto()).ToList();
    }

    public static List<BookInfoDto> ToDto(this IEnumerable<BookInfo> books)
    {
        return books.Select(b => b.ToDto()).ToList();
    }

    // Paged result mapping
    public static PagedResult<T> ToPagedResult<T>(this IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
    {
        return new PagedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    // Reading Status mapping
    public static ReadingStatus ToReadingStatus(this string status)
    {
        return status.ToLowerInvariant() switch
        {
            "toread" or "to-read" or "to_read" => ReadingStatus.ToRead,
            "reading" or "currently-reading" => ReadingStatus.Reading,
            "finished" or "read" or "completed" => ReadingStatus.Finished,
            "onhold" or "on-hold" or "on_hold" => ReadingStatus.OnHold,
            "dropped" or "abandoned" => ReadingStatus.Dropped,
            _ => throw new ArgumentException($"Invalid reading status: {status}")
        };
    }

    public static string ToDisplayString(this ReadingStatus status)
    {
        return status switch
        {
            ReadingStatus.ToRead => "To Read",
            ReadingStatus.Reading => "Currently Reading",
            ReadingStatus.Finished => "Finished",
            ReadingStatus.OnHold => "On Hold",
            ReadingStatus.Dropped => "Dropped",
            _ => status.ToString()
        };
    }
}
