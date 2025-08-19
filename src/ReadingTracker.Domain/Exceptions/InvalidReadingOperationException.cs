namespace ReadingTracker.Domain.Exceptions;

public class InvalidReadingOperationException : DomainException
{
    public InvalidReadingOperationException(string operation, string currentStatus) 
        : base($"Cannot {operation} when book status is '{currentStatus}'") { }
    
    public InvalidReadingOperationException(string message) : base(message) { }
}
