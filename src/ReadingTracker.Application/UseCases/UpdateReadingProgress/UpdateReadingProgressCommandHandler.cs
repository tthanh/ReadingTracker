using System;
using System.Threading;
using System.Threading.Tasks;
using ReadingTracker.Application.Common;
using ReadingTracker.Domain.Exceptions;
using ReadingTracker.Domain.Repositories;

namespace ReadingTracker.Application.UseCases.UpdateReadingProgress;

public class UpdateReadingProgressCommandHandler : ICommandHandler<UpdateReadingProgressCommand>
{
    private readonly IUserBookRepository _userBookRepository;

    public UpdateReadingProgressCommandHandler(IUserBookRepository userBookRepository)
    {
        _userBookRepository = userBookRepository ?? throw new ArgumentNullException(nameof(userBookRepository));
    }

    public async Task Handle(UpdateReadingProgressCommand request, CancellationToken cancellationToken)
    {
        var userBook = await _userBookRepository.GetByIdAsync(request.UserBookId);
        if (userBook == null)
        {
            throw new UserBookNotFoundException(request.UserBookId);
        }

        // Start reading if not already started
        if (userBook.Status == Domain.ValueObjects.ReadingStatus.ToRead)
        {
            userBook.StartReading();
        }

        userBook.UpdateProgress(request.PageNumber);
        await _userBookRepository.UpdateAsync(userBook);
    }
}
