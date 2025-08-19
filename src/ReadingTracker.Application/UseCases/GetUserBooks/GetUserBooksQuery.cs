using ReadingTracker.Application.Common;
using ReadingTracker.Domain.ValueObjects;

namespace ReadingTracker.Application.UseCases.GetUserBooks;

public record GetUserBooksQuery(
    Guid UserId,
    ReadingStatus? StatusFilter = null,
    string? SearchTerm = null,
    int PageNumber = 1,
    int PageSize = 20
) : IQuery<GetUserBooksResult>;
