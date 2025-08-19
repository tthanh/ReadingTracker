using ReadingTracker.Application.Common;
using ReadingTracker.Domain.Repositories;

namespace ReadingTracker.Application.UseCases.UpdatePersonalNotes;

public class UpdatePersonalNotesCommandHandler : ICommandHandler<UpdatePersonalNotesCommand>
{
    private readonly IUserBookRepository _userBookRepository;

    public UpdatePersonalNotesCommandHandler(IUserBookRepository userBookRepository)
    {
        _userBookRepository = userBookRepository;
    }

    public async Task Handle(UpdatePersonalNotesCommand request, CancellationToken cancellationToken)
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

        userBook.UpdatePersonalNotes(request.PersonalNotes);

        await _userBookRepository.UpdateAsync(userBook);
    }
}
