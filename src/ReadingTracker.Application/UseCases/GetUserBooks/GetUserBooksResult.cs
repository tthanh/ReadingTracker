using ReadingTracker.Domain.ValueObjects;

namespace ReadingTracker.Application.UseCases.GetUserBooks;

public record UserBookDto(
    Guid UserBookId,
    string BookId,
    string Title,
    string Author,
    string? Genre,
    int? TotalPages,
    ReadingStatus Status,
    int CurrentPage,
    double ProgressPercentage,
    DateTime AddedDate,
    DateTime? StartedDate,
    DateTime? FinishedDate,
    string? PersonalNotes,
    int? PersonalRating
);

public record GetUserBooksResult(
    IEnumerable<UserBookDto> Books,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);
