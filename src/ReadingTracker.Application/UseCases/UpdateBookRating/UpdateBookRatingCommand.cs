using ReadingTracker.Application.Common;

namespace ReadingTracker.Application.UseCases.UpdateBookRating;

public record UpdateBookRatingCommand(
    Guid UserId,
    Guid BookId,
    int? Rating // null to remove rating, 1-5 to set rating
) : ICommand;
