using ReadingTracker.Application.Common;
using ReadingTracker.Domain.Aggregates;
using ReadingTracker.Domain.Repositories;

namespace ReadingTracker.Application.UseCases.ChangeBookStatus;

public class ChangeBookStatusCommandHandler : ICommandHandler<ChangeBookStatusCommand, Guid>
{
    private readonly IUserBookRepository _userBookRepository;

    public ChangeBookStatusCommandHandler(IUserBookRepository userBookRepository)
    {
        _userBookRepository = userBookRepository;
    }

    public async Task<Guid> Handle(ChangeBookStatusCommand request, CancellationToken cancellationToken)
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

        // Use appropriate domain methods based on the new status
        switch (request.NewStatus)
        {
            case ReadingTracker.Domain.ValueObjects.ReadingStatus.Reading:
                if (userBook.Status == ReadingTracker.Domain.ValueObjects.ReadingStatus.OnHold)
                {
                    userBook.ResumeReading();
                }
                else
                {
                    userBook.StartReading();
                }
                break;
            
            case ReadingTracker.Domain.ValueObjects.ReadingStatus.Finished:
                userBook.MarkAsFinished();
                break;
            
            case ReadingTracker.Domain.ValueObjects.ReadingStatus.OnHold:
                userBook.PutOnHold();
                break;
            
            case ReadingTracker.Domain.ValueObjects.ReadingStatus.Dropped:
                userBook.DropBook();
                break;
            
            case ReadingTracker.Domain.ValueObjects.ReadingStatus.ToRead:
                // For setting back to ToRead, we might need a new domain method
                // For now, this might not be a common use case
                throw new InvalidOperationException("Cannot change status back to 'To Read'.");
            
            default:
                throw new ArgumentException($"Invalid reading status: {request.NewStatus}");
        }

        await _userBookRepository.UpdateAsync(userBook);

        return userBook.UserBookId;
    }
}
