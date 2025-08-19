using System;
using ReadingTracker.Application.Common;

namespace ReadingTracker.Application.UseCases.AddBookToLibrary;

public record AddBookToLibraryCommand(
    Guid UserId,
    string BookId,
    string Title,
    string Author,
    string? Genre = null,
    string? Isbn = null,
    int? TotalPages = null,
    string? Publisher = null,
    DateTime? PublishedDate = null,
    string? PersonalNotes = null
) : ICommand<Guid>;
