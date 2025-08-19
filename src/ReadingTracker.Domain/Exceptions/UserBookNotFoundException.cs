using System;

namespace ReadingTracker.Domain.Exceptions;

public class UserBookNotFoundException : DomainException
{
    public UserBookNotFoundException(Guid userBookId) 
        : base($"UserBook with ID '{userBookId}' was not found") { }
    
    public UserBookNotFoundException(Guid userId, string bookId) 
        : base($"Book '{bookId}' not found in library for user '{userId}'") { }
}
