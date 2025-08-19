using System;
using ReadingTracker.Application.Common;

namespace ReadingTracker.Application.UseCases.LogReadingSession;

public record LogReadingSessionCommand(
    Guid UserBookId,
    DateTime SessionDate,
    int StartPage,
    int EndPage,
    DateTime? EndTime = null,
    string? SessionNotes = null
) : ICommand;
