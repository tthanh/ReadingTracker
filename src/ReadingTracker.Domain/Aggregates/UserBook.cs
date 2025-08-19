using System;
using System.Collections.Generic;
using System.Linq;
using ReadingTracker.Domain.Entities;
using ReadingTracker.Domain.Events;
using ReadingTracker.Domain.ValueObjects;

namespace ReadingTracker.Domain.Aggregates;

public class UserBook
{
    public Guid UserBookId { get; private set; }
    public string BookId { get; private set; } // ISBN or other unique identifier
    public Guid UserId { get; private set; }
    public BookInfo BookInfo { get; private set; }
    public ReadingStatus Status { get; private set; }
    public Progress CurrentProgress { get; private set; }
    public DateTime AddedDate { get; private set; }
    public DateTime? StartedDate { get; private set; }
    public DateTime? FinishedDate { get; private set; }
    public string? PersonalNotes { get; private set; }
    public int? PersonalRating { get; private set; } // 1-5 stars
    
    private readonly List<ReadingSession> _readingSessions = new();
    public IReadOnlyList<ReadingSession> ReadingSessions => _readingSessions.AsReadOnly();

    private readonly List<DomainEvent> _domainEvents = new();
    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void ClearDomainEvents() => _domainEvents.Clear();

    // Private constructor for EF Core
    private UserBook() 
    {
        BookId = string.Empty;
        BookInfo = null!;
        CurrentProgress = null!;
    }

    public UserBook(
        string bookId,
        Guid userId,
        BookInfo bookInfo,
        string? personalNotes = null)
    {
        if (string.IsNullOrWhiteSpace(bookId))
            throw new ArgumentException("Book ID cannot be empty", nameof(bookId));
        
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        UserBookId = Guid.NewGuid();
        BookId = bookId.Trim();
        UserId = userId;
        BookInfo = bookInfo ?? throw new ArgumentNullException(nameof(bookInfo));
        Status = ReadingStatus.ToRead;
        CurrentProgress = bookInfo.TotalPages.HasValue 
            ? Progress.FromPage(0, bookInfo.TotalPages.Value)
            : Progress.FromPage(0);
        AddedDate = DateTime.UtcNow;
        PersonalNotes = personalNotes?.Trim();

        // Raise domain event
        _domainEvents.Add(new BookAddedToLibraryEvent(UserBookId, UserId, BookId, BookInfo));
    }

    public void StartReading(DateTime? startDate = null)
    {
        if (Status == ReadingStatus.Reading)
            throw new InvalidOperationException("Book is already being read");
        
        if (Status == ReadingStatus.Finished)
            throw new InvalidOperationException("Cannot start reading a finished book");

        Status = ReadingStatus.Reading;
        StartedDate = startDate ?? DateTime.UtcNow;
    }

    public void LogReadingSession(
        DateTime sessionDate,
        int startPage,
        int endPage,
        DateTime? endTime = null,
        string? sessionNotes = null)
    {
        if (Status != ReadingStatus.Reading)
            throw new InvalidOperationException("Cannot log session for a book that is not being read");

        var session = new ReadingSession(sessionDate, startPage, endPage, endTime, sessionNotes);
        _readingSessions.Add(session);

        // Update current progress to the highest page reached
        var highestPage = Math.Max(CurrentProgress.PageNumber, endPage);
        CurrentProgress = BookInfo.TotalPages.HasValue 
            ? Progress.FromPage(highestPage, BookInfo.TotalPages.Value)
            : Progress.FromPage(highestPage);

        // Auto-complete if reached the end
        if (CurrentProgress.IsComplete)
        {
            MarkAsFinished();
        }

        // Raise domain event
        _domainEvents.Add(new ReadingSessionLoggedEvent(
            UserBookId, UserId, session.SessionId, sessionDate, session.PagesRead, CurrentProgress));
    }

    public void UpdateProgress(int pageNumber)
    {
        if (Status != ReadingStatus.Reading)
            throw new InvalidOperationException("Cannot update progress for a book that is not being read");

        CurrentProgress = BookInfo.TotalPages.HasValue 
            ? Progress.FromPage(pageNumber, BookInfo.TotalPages.Value)
            : Progress.FromPage(pageNumber);

        if (CurrentProgress.IsComplete)
        {
            MarkAsFinished();
        }
    }

    public void MarkAsFinished(DateTime? finishedDate = null)
    {
        if (Status == ReadingStatus.Finished)
            throw new InvalidOperationException("Book is already finished");

        Status = ReadingStatus.Finished;
        FinishedDate = finishedDate ?? DateTime.UtcNow;

        // Ensure progress shows completion
        if (BookInfo.TotalPages.HasValue)
        {
            CurrentProgress = Progress.FromPage(BookInfo.TotalPages.Value, BookInfo.TotalPages.Value);
        }

        // Raise domain event
        _domainEvents.Add(new BookFinishedEvent(
            UserBookId, UserId, BookId, BookInfo, FinishedDate.Value, TotalReadingTime, TotalPagesRead));
    }

    public void PutOnHold()
    {
        if (Status == ReadingStatus.Finished)
            throw new InvalidOperationException("Cannot put a finished book on hold");

        Status = ReadingStatus.OnHold;
    }

    public void DropBook()
    {
        if (Status == ReadingStatus.Finished)
            throw new InvalidOperationException("Cannot drop a finished book");

        Status = ReadingStatus.Dropped;
    }

    public void ResumeReading()
    {
        if (Status != ReadingStatus.OnHold)
            throw new InvalidOperationException("Can only resume books that are on hold");

        Status = ReadingStatus.Reading;
    }

    public void UpdatePersonalNotes(string? notes)
    {
        PersonalNotes = notes?.Trim();
    }

    public void RateBook(int rating)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentException("Rating must be between 1 and 5", nameof(rating));

        PersonalRating = rating;
    }

    public void RemoveRating()
    {
        PersonalRating = null;
    }

    // Statistics and calculated properties
    public int TotalPagesRead => _readingSessions.Sum(s => s.PagesRead);
    
    public TimeSpan TotalReadingTime => TimeSpan.FromTicks(
        _readingSessions.Where(s => s.Duration.HasValue).Sum(s => s.Duration!.Value.Ticks));
    
    public int TotalReadingSessions => _readingSessions.Count;
    
    public DateTime? LastReadingSessionDate => _readingSessions
        .OrderByDescending(s => s.StartDate)
        .FirstOrDefault()?.StartDate;

    public TimeSpan? TimeSinceLastSession => LastReadingSessionDate.HasValue 
        ? DateTime.UtcNow - LastReadingSessionDate.Value 
        : null;

    public override string ToString() => $"{BookInfo.Title} - {Status} ({CurrentProgress})";
}
