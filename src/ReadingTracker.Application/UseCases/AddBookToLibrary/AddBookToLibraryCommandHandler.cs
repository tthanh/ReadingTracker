using System;
using System.Threading;
using System.Threading.Tasks;
using ReadingTracker.Application.Common;
using ReadingTracker.Domain.Aggregates;
using ReadingTracker.Domain.Repositories;
using ReadingTracker.Domain.ValueObjects;

namespace ReadingTracker.Application.UseCases.AddBookToLibrary;

public class AddBookToLibraryCommandHandler : ICommandHandler<AddBookToLibraryCommand, Guid>
{
    private readonly IUserBookRepository _userBookRepository;

    public AddBookToLibraryCommandHandler(IUserBookRepository userBookRepository)
    {
        _userBookRepository = userBookRepository ?? throw new ArgumentNullException(nameof(userBookRepository));
    }

    public async Task<Guid> Handle(AddBookToLibraryCommand request, CancellationToken cancellationToken)
    {
        // Check if user already has this book
        var existingBook = await _userBookRepository.GetByUserAndBookAsync(request.UserId, request.BookId);
        if (existingBook != null)
        {
            throw new InvalidOperationException($"Book '{request.BookId}' is already in the user's library");
        }

        // Create BookInfo value object
        var bookInfo = new BookInfo(
            request.Title,
            request.Author,
            request.Genre,
            request.Isbn,
            request.TotalPages,
            request.Publisher,
            request.PublishedDate
        );

        // Create UserBook aggregate
        var userBook = new UserBook(
            request.BookId,
            request.UserId,
            bookInfo,
            request.PersonalNotes
        );

        // Save to repository
        await _userBookRepository.AddAsync(userBook);

        return userBook.UserBookId;
    }
}
