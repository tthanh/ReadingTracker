using ReadingTracker.Application.Common;
using ReadingTracker.Domain.Repositories;

namespace ReadingTracker.Application.UseCases.GetUserBooks;

public class GetUserBooksQueryHandler : IQueryHandler<GetUserBooksQuery, GetUserBooksResult>
{
    private readonly IUserBookRepository _userBookRepository;

    public GetUserBooksQueryHandler(IUserBookRepository userBookRepository)
    {
        _userBookRepository = userBookRepository;
    }

    public async Task<GetUserBooksResult> Handle(GetUserBooksQuery request, CancellationToken cancellationToken)
    {
        var (books, totalCount) = await _userBookRepository.GetPagedAsync(
            request.UserId,
            request.PageNumber,
            request.PageSize,
            request.StatusFilter,
            request.SearchTerm);

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

        return new GetUserBooksResult(
            bookDtos,
            totalCount,
            request.PageNumber,
            request.PageSize,
            totalPages);
    }
}
