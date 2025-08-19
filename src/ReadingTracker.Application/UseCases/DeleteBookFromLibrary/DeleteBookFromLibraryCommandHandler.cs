using ReadingTracker.Application.Common;
using ReadingTracker.Domain.Repositories;

namespace ReadingTracker.Application.UseCases.DeleteBookFromLibrary;

public class DeleteBookFromLibraryCommandHandler : ICommandHandler<DeleteBookFromLibraryCommand>
{
    private readonly IUserBookRepository _userBookRepository;

    public DeleteBookFromLibraryCommandHandler(IUserBookRepository userBookRepository)
    {
        _userBookRepository = userBookRepository;
    }

    public async Task Handle(DeleteBookFromLibraryCommand request, CancellationToken cancellationToken)
    {
        var userBook = await _userBookRepository.GetByIdAsync(request.BookId);
        
        if (userBook == null)
        {
            throw new InvalidOperationException($"Book with ID {request.BookId} not found.");
        }

        if (userBook.UserId != request.UserId)
        {
            throw new UnauthorizedAccessException("User is not authorized to delete this book.");
        }

        await _userBookRepository.DeleteAsync(request.BookId);
    }
}
