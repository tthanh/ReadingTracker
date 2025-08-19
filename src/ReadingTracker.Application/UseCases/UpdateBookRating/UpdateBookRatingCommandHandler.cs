using ReadingTracker.Application.Common;
using ReadingTracker.Domain.Repositories;

namespace ReadingTracker.Application.UseCases.UpdateBookRating;

public class UpdateBookRatingCommandHandler : ICommandHandler<UpdateBookRatingCommand>
{
    private readonly IUserBookRepository _userBookRepository;

    public UpdateBookRatingCommandHandler(IUserBookRepository userBookRepository)
    {
        _userBookRepository = userBookRepository;
    }

    public async Task Handle(UpdateBookRatingCommand request, CancellationToken cancellationToken)
    {
        var userBook = await _userBookRepository.GetByIdAsync(request.BookId);
        
        if (userBook == null)
        {
            throw new InvalidOperationException($"Book with ID {request.BookId} not found.");
        }

        if (userBook.UserId != request.UserId)
        {
            throw new UnauthorizedAccessException("User is not authorized to modify this book.");
        }

        if (request.Rating.HasValue)
        {
            userBook.RateBook(request.Rating.Value);
        }
        else
        {
            userBook.RemoveRating();
        }

        await _userBookRepository.UpdateAsync(userBook);
    }
}
