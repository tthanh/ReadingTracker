using Microsoft.AspNetCore.Mvc;
using ReadingTracker.Api.Extensions;
using ReadingTracker.Api.Models;
using ReadingTracker.Domain.Aggregates;
using ReadingTracker.Domain.Exceptions;
using ReadingTracker.Domain.Repositories;
using ReadingTracker.Domain.ValueObjects;

namespace ReadingTracker.Api.Controllers;

/// <summary>
/// Manages user's personal book library
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class UserBooksController : BaseController
{
    private readonly IUserBookRepository _userBookRepository;
    private readonly ILogger<UserBooksController> _logger;

    public UserBooksController(
        IUserBookRepository userBookRepository,
        ILogger<UserBooksController> logger)
    {
        _userBookRepository = userBookRepository ?? throw new ArgumentNullException(nameof(userBookRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all books in user's library
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<UserBookDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<UserBookDto>>>> GetUserBooks()
    {
        try
        {
            var userId = GetCurrentUserId();
            var userBooks = await _userBookRepository.GetByUserIdAsync(userId);
            var userBookDtos = userBooks.ToDto();

            return Success(userBookDtos, $"Retrieved {userBookDtos.Count} books");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user books");
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get a specific book from user's library
    /// </summary>
    [HttpGet("{userBookId}")]
    [ProducesResponseType(typeof(ApiResponse<UserBookDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserBookDto>>> GetUserBook(Guid userBookId)
    {
        try
        {
            var userBook = await _userBookRepository.GetByIdAsync(userBookId);
            
            if (userBook == null)
                return Error<UserBookDto>("Book not found", StatusCodes.Status404NotFound);

            // Verify the book belongs to the current user
            var userId = GetCurrentUserId();
            if (userBook.UserId != userId)
                return Error<UserBookDto>("Book not found", StatusCodes.Status404NotFound);

            return Success(userBook.ToDto());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user book {UserBookId}", userBookId);
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Add a book to user's library
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<UserBookDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<UserBookDto>>> AddBookToLibrary([FromBody] AddBookToLibraryRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return ValidationError<UserBookDto>(errors);
            }

            var userId = GetCurrentUserId();

            // Check if book already exists in user's library
            var existingBook = await _userBookRepository.GetByUserAndBookAsync(userId, request.BookId);
            if (existingBook != null)
                return Error<UserBookDto>("Book already exists in your library", StatusCodes.Status409Conflict);

            // Create the UserBook
            var bookInfo = request.BookInfo.ToDomain();
            var userBook = new UserBook(request.BookId, userId, bookInfo, request.PersonalNotes);

            await _userBookRepository.AddAsync(userBook);

            _logger.LogInformation("Added book {BookId} to library for user {UserId}", request.BookId, userId);

            return CreatedAtAction(
                nameof(GetUserBook), 
                new { userBookId = userBook.UserBookId }, 
                Success(userBook.ToDto(), "Book added to library successfully").Value);
        }
        catch (ArgumentException ex)
        {
            return Error<UserBookDto>(ex.Message, StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding book to library");
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Update book reading status
    /// </summary>
    [HttpPut("{userBookId}/status")]
    [ProducesResponseType(typeof(ApiResponse<UserBookDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<UserBookDto>>> UpdateBookStatus(
        Guid userBookId, 
        [FromBody] UpdateBookStatusRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return ValidationError<UserBookDto>(errors);
            }

            var userBook = await GetUserBookAndValidateOwnership(userBookId);
            if (userBook == null)
                return Error<UserBookDto>("Book not found", StatusCodes.Status404NotFound);

            var status = request.Status.ToReadingStatus();

            // Update status based on the requested status
            switch (status)
            {
                case ReadingStatus.Reading:
                    userBook.StartReading(request.Date);
                    break;
                case ReadingStatus.Finished:
                    userBook.MarkAsFinished(request.Date);
                    break;
                case ReadingStatus.OnHold:
                    userBook.PutOnHold();
                    break;
                case ReadingStatus.Dropped:
                    userBook.DropBook();
                    break;
                case ReadingStatus.ToRead:
                    // If currently on hold, resume reading first, then change to ToRead
                    if (userBook.Status == ReadingStatus.OnHold)
                        userBook.ResumeReading();
                    // Note: There's no direct way to set back to ToRead in the domain model
                    // This might need to be handled differently based on business rules
                    break;
            }

            await _userBookRepository.UpdateAsync(userBook);

            _logger.LogInformation("Updated status for book {UserBookId} to {Status}", userBookId, status);

            return Success(userBook.ToDto(), "Book status updated successfully");
        }
        catch (InvalidOperationException ex)
        {
            return Error<UserBookDto>(ex.Message, StatusCodes.Status400BadRequest);
        }
        catch (ArgumentException ex)
        {
            return Error<UserBookDto>(ex.Message, StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating book status for {UserBookId}", userBookId);
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Log a reading session
    /// </summary>
    [HttpPost("{userBookId}/sessions")]
    [ProducesResponseType(typeof(ApiResponse<UserBookDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<UserBookDto>>> LogReadingSession(
        Guid userBookId, 
        [FromBody] LogReadingSessionRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return ValidationError<UserBookDto>(errors);
            }

            var userBook = await GetUserBookAndValidateOwnership(userBookId);
            if (userBook == null)
                return Error<UserBookDto>("Book not found", StatusCodes.Status404NotFound);

            userBook.LogReadingSession(
                request.SessionDate,
                request.StartPage,
                request.EndPage,
                request.EndTime,
                request.SessionNotes);

            await _userBookRepository.UpdateAsync(userBook);

            _logger.LogInformation("Logged reading session for book {UserBookId}: {StartPage}-{EndPage}", 
                userBookId, request.StartPage, request.EndPage);

            return Success(userBook.ToDto(), "Reading session logged successfully");
        }
        catch (InvalidOperationException ex)
        {
            return Error<UserBookDto>(ex.Message, StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging reading session for {UserBookId}", userBookId);
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Update reading progress
    /// </summary>
    [HttpPut("{userBookId}/progress")]
    [ProducesResponseType(typeof(ApiResponse<UserBookDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<UserBookDto>>> UpdateReadingProgress(
        Guid userBookId, 
        [FromBody] UpdateReadingProgressRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return ValidationError<UserBookDto>(errors);
            }

            var userBook = await GetUserBookAndValidateOwnership(userBookId);
            if (userBook == null)
                return Error<UserBookDto>("Book not found", StatusCodes.Status404NotFound);

            userBook.UpdateProgress(request.PageNumber);

            await _userBookRepository.UpdateAsync(userBook);

            _logger.LogInformation("Updated progress for book {UserBookId} to page {PageNumber}", 
                userBookId, request.PageNumber);

            return Success(userBook.ToDto(), "Reading progress updated successfully");
        }
        catch (InvalidOperationException ex)
        {
            return Error<UserBookDto>(ex.Message, StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating reading progress for {UserBookId}", userBookId);
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Update personal notes for a book
    /// </summary>
    [HttpPut("{userBookId}/notes")]
    [ProducesResponseType(typeof(ApiResponse<UserBookDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserBookDto>>> UpdatePersonalNotes(
        Guid userBookId, 
        [FromBody] UpdatePersonalNotesRequest request)
    {
        try
        {
            var userBook = await GetUserBookAndValidateOwnership(userBookId);
            if (userBook == null)
                return Error<UserBookDto>("Book not found", StatusCodes.Status404NotFound);

            userBook.UpdatePersonalNotes(request.PersonalNotes);

            await _userBookRepository.UpdateAsync(userBook);

            _logger.LogInformation("Updated personal notes for book {UserBookId}", userBookId);

            return Success(userBook.ToDto(), "Personal notes updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating personal notes for {UserBookId}", userBookId);
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Rate a book
    /// </summary>
    [HttpPut("{userBookId}/rating")]
    [ProducesResponseType(typeof(ApiResponse<UserBookDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<UserBookDto>>> UpdateBookRating(
        Guid userBookId, 
        [FromBody] UpdateBookRatingRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return ValidationError<UserBookDto>(errors);
            }

            var userBook = await GetUserBookAndValidateOwnership(userBookId);
            if (userBook == null)
                return Error<UserBookDto>("Book not found", StatusCodes.Status404NotFound);

            userBook.RateBook(request.Rating);

            await _userBookRepository.UpdateAsync(userBook);

            _logger.LogInformation("Updated rating for book {UserBookId} to {Rating}", userBookId, request.Rating);

            return Success(userBook.ToDto(), "Book rating updated successfully");
        }
        catch (ArgumentException ex)
        {
            return Error<UserBookDto>(ex.Message, StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating book rating for {UserBookId}", userBookId);
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Remove rating from a book
    /// </summary>
    [HttpDelete("{userBookId}/rating")]
    [ProducesResponseType(typeof(ApiResponse<UserBookDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserBookDto>>> RemoveBookRating(Guid userBookId)
    {
        try
        {
            var userBook = await GetUserBookAndValidateOwnership(userBookId);
            if (userBook == null)
                return Error<UserBookDto>("Book not found", StatusCodes.Status404NotFound);

            userBook.RemoveRating();

            await _userBookRepository.UpdateAsync(userBook);

            _logger.LogInformation("Removed rating for book {UserBookId}", userBookId);

            return Success(userBook.ToDto(), "Book rating removed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing book rating for {UserBookId}", userBookId);
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Delete a book from user's library
    /// </summary>
    [HttpDelete("{userBookId}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteBook(Guid userBookId)
    {
        try
        {
            var userBook = await GetUserBookAndValidateOwnership(userBookId);
            if (userBook == null)
                return Error("Book not found", StatusCodes.Status404NotFound);

            await _userBookRepository.DeleteAsync(userBookId);

            _logger.LogInformation("Deleted book {UserBookId} from library", userBookId);

            return Success("Book deleted from library successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting book {UserBookId}", userBookId);
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get books by status
    /// </summary>
    [HttpGet("by-status/{status}")]
    [ProducesResponseType(typeof(ApiResponse<List<UserBookDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<List<UserBookDto>>>> GetBooksByStatus(string status)
    {
        try
        {
            var readingStatus = status.ToReadingStatus();
            var userId = GetCurrentUserId();
            var userBooks = await _userBookRepository.FindByStatusAsync(userId, readingStatus);
            var userBookDtos = userBooks.ToDto();

            return Success(userBookDtos, $"Retrieved {userBookDtos.Count} books with status '{readingStatus.ToDisplayString()}'");
        }
        catch (ArgumentException ex)
        {
            return Error<List<UserBookDto>>(ex.Message, StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving books by status {Status}", status);
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Search books in user's library
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<List<UserBookDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<UserBookDto>>>> SearchUserBooks([FromQuery] string query)
    {
        try
        {
            var userId = GetCurrentUserId();
            var userBooks = await _userBookRepository.SearchAsync(userId, query);
            var userBookDtos = userBooks.ToDto();

            return Success(userBookDtos, $"Found {userBookDtos.Count} books matching '{query}'");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching user books with query {Query}", query);
            return HandleException(ex);
        }
    }

    private async Task<UserBook?> GetUserBookAndValidateOwnership(Guid userBookId)
    {
        var userBook = await _userBookRepository.GetByIdAsync(userBookId);
        
        if (userBook == null)
            return null;

        var userId = GetCurrentUserId();
        if (userBook.UserId != userId)
            return null;

        return userBook;
    }
}
