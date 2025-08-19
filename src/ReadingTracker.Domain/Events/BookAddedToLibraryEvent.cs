using System;
using ReadingTracker.Domain.ValueObjects;

namespace ReadingTracker.Domain.Events;

public record BookAddedToLibraryEvent(
    Guid UserBookId,
    Guid UserId,
    string BookId,
    BookInfo BookInfo
) : DomainEvent;
