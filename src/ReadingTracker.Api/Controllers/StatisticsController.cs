using Microsoft.AspNetCore.Mvc;
using ReadingTracker.Api.Extensions;
using ReadingTracker.Api.Models;
using ReadingTracker.Domain.Repositories;
using ReadingTracker.Domain.ValueObjects;

namespace ReadingTracker.Api.Controllers;

/// <summary>
/// Provides reading statistics and analytics
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class StatisticsController : BaseController
{
    private readonly IUserBookRepository _userBookRepository;
    private readonly ILogger<StatisticsController> _logger;

    public StatisticsController(
        IUserBookRepository userBookRepository,
        ILogger<StatisticsController> logger)
    {
        _userBookRepository = userBookRepository ?? throw new ArgumentNullException(nameof(userBookRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get comprehensive reading statistics for the user
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<ReadingStatisticsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ReadingStatisticsDto>>> GetReadingStatistics()
    {
        try
        {
            var userId = GetCurrentUserId();

            // Get all user books
            var userBooks = await _userBookRepository.GetByUserIdAsync(userId);
            var userBooksList = userBooks.ToList();

            // Calculate statistics
            var totalBooks = userBooksList.Count;
            var booksRead = userBooksList.Count(b => b.Status == ReadingStatus.Finished);
            var booksCurrentlyReading = userBooksList.Count(b => b.Status == ReadingStatus.Reading);
            var booksToRead = userBooksList.Count(b => b.Status == ReadingStatus.ToRead);
            var booksOnHold = userBooksList.Count(b => b.Status == ReadingStatus.OnHold);
            var booksDropped = userBooksList.Count(b => b.Status == ReadingStatus.Dropped);

            var totalPagesRead = userBooksList.Sum(b => b.TotalPagesRead);
            var totalReadingTime = TimeSpan.FromTicks(userBooksList.Sum(b => b.TotalReadingTime.Ticks));
            var totalReadingSessions = userBooksList.Sum(b => b.TotalReadingSessions);

            var ratedBooks = userBooksList.Where(b => b.PersonalRating.HasValue).ToList();
            var averageRating = ratedBooks.Any() ? ratedBooks.Average(b => b.PersonalRating!.Value) : 0;

            var lastReadingSession = userBooksList
                .Where(b => b.LastReadingSessionDate.HasValue)
                .OrderByDescending(b => b.LastReadingSessionDate)
                .FirstOrDefault()?.LastReadingSessionDate;

            // Get recently finished books (last 30 days)
            var recentlyFinished = await _userBookRepository.FindRecentlyFinishedAsync(userId, 30);
            var currentlyReading = await _userBookRepository.FindCurrentlyReadingAsync(userId);

            var statistics = new ReadingStatisticsDto
            {
                TotalBooks = totalBooks,
                BooksRead = booksRead,
                BooksCurrentlyReading = booksCurrentlyReading,
                BooksToRead = booksToRead,
                BooksOnHold = booksOnHold,
                BooksDropped = booksDropped,
                TotalPagesRead = totalPagesRead,
                TotalReadingTime = totalReadingTime,
                AverageRating = Math.Round(averageRating, 2),
                TotalReadingSessions = totalReadingSessions,
                LastReadingSession = lastReadingSession,
                RecentlyFinished = recentlyFinished.ToDto(),
                CurrentlyReading = currentlyReading.ToDto()
            };

            _logger.LogInformation("Generated reading statistics for user {UserId}", userId);

            return Success(statistics, "Reading statistics retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating reading statistics");
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get books by reading status
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
    /// Get recently finished books
    /// </summary>
    [HttpGet("recently-finished")]
    [ProducesResponseType(typeof(ApiResponse<List<UserBookDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<UserBookDto>>>> GetRecentlyFinishedBooks([FromQuery] int days = 30)
    {
        try
        {
            if (days < 1 || days > 365)
                return Error<List<UserBookDto>>("Days must be between 1 and 365", StatusCodes.Status400BadRequest);

            var userId = GetCurrentUserId();
            var userBooks = await _userBookRepository.FindRecentlyFinishedAsync(userId, days);
            var userBookDtos = userBooks.ToDto();

            return Success(userBookDtos, $"Retrieved {userBookDtos.Count} books finished in the last {days} days");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recently finished books");
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get books by rating
    /// </summary>
    [HttpGet("by-rating/{rating}")]
    [ProducesResponseType(typeof(ApiResponse<List<UserBookDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<List<UserBookDto>>>> GetBooksByRating(int rating)
    {
        try
        {
            if (rating < 1 || rating > 5)
                return Error<List<UserBookDto>>("Rating must be between 1 and 5", StatusCodes.Status400BadRequest);

            var userId = GetCurrentUserId();
            var userBooks = await _userBookRepository.FindByRatingAsync(userId, rating);
            var userBookDtos = userBooks.ToDto();

            return Success(userBookDtos, $"Retrieved {userBookDtos.Count} books with {rating}-star rating");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving books by rating {Rating}", rating);
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get books by author
    /// </summary>
    [HttpGet("by-author")]
    [ProducesResponseType(typeof(ApiResponse<List<UserBookDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<List<UserBookDto>>>> GetBooksByAuthor([FromQuery] string author)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(author))
                return Error<List<UserBookDto>>("Author name is required", StatusCodes.Status400BadRequest);

            var userId = GetCurrentUserId();
            var userBooks = await _userBookRepository.FindByAuthorAsync(userId, author);
            var userBookDtos = userBooks.ToDto();

            return Success(userBookDtos, $"Retrieved {userBookDtos.Count} books by author '{author}'");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving books by author {Author}", author);
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get simple statistics counts
    /// </summary>
    [HttpGet("counts")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> GetStatisticsCounts()
    {
        try
        {
            var userId = GetCurrentUserId();

            var totalBooks = await _userBookRepository.GetTotalBooksCountAsync(userId);
            var toReadCount = await _userBookRepository.GetBooksCountByStatusAsync(userId, ReadingStatus.ToRead);
            var readingCount = await _userBookRepository.GetBooksCountByStatusAsync(userId, ReadingStatus.Reading);
            var finishedCount = await _userBookRepository.GetBooksCountByStatusAsync(userId, ReadingStatus.Finished);
            var onHoldCount = await _userBookRepository.GetBooksCountByStatusAsync(userId, ReadingStatus.OnHold);
            var droppedCount = await _userBookRepository.GetBooksCountByStatusAsync(userId, ReadingStatus.Dropped);

            var counts = new
            {
                TotalBooks = totalBooks,
                ToRead = toReadCount,
                Reading = readingCount,
                Finished = finishedCount,
                OnHold = onHoldCount,
                Dropped = droppedCount
            };

            return Success((object)counts, "Statistics counts retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving statistics counts");
            return HandleException(ex);
        }
    }
}
