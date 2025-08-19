using System;
using ReadingTracker.Domain.ValueObjects;

namespace ReadingTracker.Domain.Events;

public record ReadingSessionLoggedEvent(
    Guid UserBookId,
    Guid UserId,
    Guid SessionId,
    DateTime SessionDate,
    int PagesRead,
    Progress NewProgress
) : DomainEvent;
