using System;
using ReadingTracker.Application.Common;

namespace ReadingTracker.Application.UseCases.UpdateReadingProgress;

public record UpdateReadingProgressCommand(
    Guid UserBookId,
    int PageNumber
) : ICommand;
