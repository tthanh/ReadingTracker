using ReadingTracker.Application.Common;
using ReadingTracker.Application.UseCases.GetUserBooks;
using ReadingTracker.Domain.Repositories;

namespace ReadingTracker.Application.UseCases.SearchBooks;

public class SearchBooksQueryHandler : IQueryHandler<SearchBooksQuery, SearchBooksResult>
{
    private readonly IUserBookRepository _userBookRepository;

    public SearchBooksQueryHandler(IUserBookRepository userBookRepository)
    {
        _userBookRepository = userBookRepository;
    }

    public async Task<SearchBooksResult> Handle(SearchBooksQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            return new SearchBooksResult(
                Enumerable.Empty<UserBookDto>(),
                0,
                request.PageNumber,
                request.PageSize,
                0,
                request.SearchTerm
            );
        }

        // Use the existing GetPagedAsync method with search term
        var (books, totalCount) = await _userBookRepository.GetPagedAsync(
            request.UserId,
            request.PageNumber,
            request.PageSize,
            searchTerm: request.SearchTerm);

        var bookDtos = books.Select(book => new UserBookDto(
            book.UserBookId,
            book.BookId,
            book.BookInfo.Title,
            book.BookInfo.Author,
            book.BookInfo.Genre,
            book.BookInfo.TotalPages,
            book.Status,
            book.CurrentProgress.PageNumber,
            (double)(book.CurrentProgress.Percentage ?? 0),
            book.AddedDate,
            book.StartedDate,
            book.FinishedDate,
            book.PersonalNotes,
            book.PersonalRating
        ));

        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        return new SearchBooksResult(
            bookDtos,
            totalCount,
            request.PageNumber,
            request.PageSize,
            totalPages,
            request.SearchTerm);
    }
}
