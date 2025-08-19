using System;
using ReadingTracker.Domain.ValueObjects;

namespace ReadingTracker.Domain.Events;

public record BookFinishedEvent(
    Guid UserBookId,
    Guid UserId,
    string BookId,
    BookInfo BookInfo,
    DateTime FinishedDate,
    TimeSpan TotalReadingTime,
    int TotalPagesRead
) : DomainEvent;
