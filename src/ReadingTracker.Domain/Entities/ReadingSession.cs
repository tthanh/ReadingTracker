using System;

namespace ReadingTracker.Domain.Entities;

public class ReadingSession
{
    public Guid SessionId { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public TimeSpan? Duration { get; private set; }
    public int StartPage { get; private set; }
    public int EndPage { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Private constructor for EF Core
    private ReadingSession() { }

    public ReadingSession(
        DateTime startDate,
        int startPage,
        int endPage,
        DateTime? endDate = null,
        string? notes = null)
    {
        if (startPage < 0)
            throw new ArgumentException("Start page cannot be negative", nameof(startPage));
        
        if (endPage < startPage)
            throw new ArgumentException("End page cannot be less than start page", nameof(endPage));
        
        if (endDate.HasValue && endDate < startDate)
            throw new ArgumentException("End date cannot be before start date", nameof(endDate));

        SessionId = Guid.NewGuid();
        StartDate = startDate;
        EndDate = endDate;
        StartPage = startPage;
        EndPage = endPage;
        Notes = notes?.Trim();
        CreatedAt = DateTime.UtcNow;
        
        Duration = endDate.HasValue ? endDate.Value - startDate : null;
    }

    public void CompleteSession(DateTime endDate, string? notes = null)
    {
        if (endDate < StartDate)
            throw new ArgumentException("End date cannot be before start date", nameof(endDate));
        
        if (EndDate.HasValue)
            throw new InvalidOperationException("Session is already completed");

        EndDate = endDate;
        Duration = endDate - StartDate;
        
        if (!string.IsNullOrWhiteSpace(notes))
            Notes = notes.Trim();
    }

    public void UpdateNotes(string? notes)
    {
        Notes = notes?.Trim();
    }

    public int PagesRead => EndPage - StartPage;
    
    public bool IsCompleted => EndDate.HasValue;

    public override string ToString()
    {
        var pagesRead = PagesRead;
        var durationStr = Duration?.ToString(@"hh\:mm") ?? "ongoing";
        return $"Session on {StartDate:yyyy-MM-dd}: {pagesRead} pages ({durationStr})";
    }
}
