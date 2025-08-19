using ReadingTracker.Application.Common;

namespace ReadingTracker.Application.UseCases.SearchBooks;

public record SearchBooksQuery(
    Guid UserId,
    string SearchTerm,
    int PageNumber = 1,
    int PageSize = 20
) : IQuery<SearchBooksResult>;
