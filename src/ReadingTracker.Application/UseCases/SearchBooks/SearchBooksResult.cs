using ReadingTracker.Application.UseCases.GetUserBooks;

namespace ReadingTracker.Application.UseCases.SearchBooks;

public record SearchBooksResult(
    IEnumerable<UserBookDto> Books,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages,
    string SearchTerm
);
