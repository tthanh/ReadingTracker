using ReadingTracker.Domain.ValueObjects;

namespace ReadingTracker.Application.Common.Models;

public record UserBookDto(
    Guid UserBookId,
    string BookId,
    string Title,
    string Author,
    string? Genre,
    int? TotalPages,
    ReadingStatus Status,
    int CurrentPage,
    double PercentageComplete,
    DateTime AddedDate,
    DateTime? StartedDate,
    DateTime? FinishedDate,
    string? PersonalNotes,
    int? PersonalRating
);
