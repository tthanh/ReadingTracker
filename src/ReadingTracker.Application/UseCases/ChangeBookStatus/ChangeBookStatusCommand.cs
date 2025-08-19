using ReadingTracker.Application.Common;
using ReadingTracker.Domain.ValueObjects;

namespace ReadingTracker.Application.UseCases.ChangeBookStatus;

public record ChangeBookStatusCommand(
    Guid UserId,
    Guid BookId,
    ReadingStatus NewStatus
) : ICommand<Guid>;
