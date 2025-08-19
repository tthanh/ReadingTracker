using System;
using System.Threading;
using System.Threading.Tasks;
using ReadingTracker.Application.Common;
using ReadingTracker.Domain.Exceptions;
using ReadingTracker.Domain.Repositories;

namespace ReadingTracker.Application.UseCases.LogReadingSession;

public class LogReadingSessionCommandHandler : ICommandHandler<LogReadingSessionCommand>
{
    private readonly IUserBookRepository _userBookRepository;

    public LogReadingSessionCommandHandler(IUserBookRepository userBookRepository)
    {
        _userBookRepository = userBookRepository ?? throw new ArgumentNullException(nameof(userBookRepository));
    }

    public async Task Handle(LogReadingSessionCommand request, CancellationToken cancellationToken)
    {
        // Load the UserBook aggregate
        var userBook = await _userBookRepository.GetByIdAsync(request.UserBookId);
        if (userBook == null)
        {
            throw new UserBookNotFoundException(request.UserBookId);
        }

        // Start reading if not already started
        if (userBook.Status == Domain.ValueObjects.ReadingStatus.ToRead)
        {
            userBook.StartReading(request.SessionDate);
        }

        // Log the reading session
        userBook.LogReadingSession(
            request.SessionDate,
            request.StartPage,
            request.EndPage,
            request.EndTime,
            request.SessionNotes
        );

        // Save changes
        await _userBookRepository.UpdateAsync(userBook);
    }
}
