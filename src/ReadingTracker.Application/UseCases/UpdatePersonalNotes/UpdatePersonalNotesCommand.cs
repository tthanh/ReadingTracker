using ReadingTracker.Application.Common;

namespace ReadingTracker.Application.UseCases.UpdatePersonalNotes;

public record UpdatePersonalNotesCommand(
    Guid UserId,
    Guid BookId,
    string? PersonalNotes
) : ICommand;
