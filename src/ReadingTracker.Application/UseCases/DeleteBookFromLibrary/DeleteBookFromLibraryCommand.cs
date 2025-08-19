using ReadingTracker.Application.Common;

namespace ReadingTracker.Application.UseCases.DeleteBookFromLibrary;

public record DeleteBookFromLibraryCommand(
    Guid UserId,
    Guid BookId
) : ICommand;
