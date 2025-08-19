using Microsoft.AspNetCore.Mvc;
using ReadingTracker.Api.Extensions;
using ReadingTracker.Api.Models;
using ReadingTracker.Infrastructure.ExternalServices;

namespace ReadingTracker.Api.Controllers;

/// <summary>
/// Search for books from external sources
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class BookSearchController : BaseController
{
    private readonly IBookSearchService _bookSearchService;
    private readonly ILogger<BookSearchController> _logger;

    public BookSearchController(
        IBookSearchService bookSearchService,
        ILogger<BookSearchController> logger)
    {
        _bookSearchService = bookSearchService ?? throw new ArgumentNullException(nameof(bookSearchService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Search for books by title, author, or keywords
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<List<BookInfoDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<List<BookInfoDto>>>> SearchBooks(
        [FromQuery] string query,
        [FromQuery] int maxResults = 10)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
                return Error<List<BookInfoDto>>("Search query is required", StatusCodes.Status400BadRequest);

            if (maxResults < 1 || maxResults > 40)
                return Error<List<BookInfoDto>>("Max results must be between 1 and 40", StatusCodes.Status400BadRequest);

            var books = await _bookSearchService.SearchBooksAsync(query, maxResults);
            var bookDtos = books.ToDto();

            _logger.LogInformation("Book search completed for query '{Query}', found {Count} results", query, bookDtos.Count);

            return Success(bookDtos, $"Found {bookDtos.Count} books matching '{query}'");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching books with query '{Query}'", query);
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Search for a book by ISBN
    /// </summary>
    [HttpGet("isbn/{isbn}")]
    [ProducesResponseType(typeof(ApiResponse<BookInfoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<BookInfoDto>>> GetBookByIsbn(string isbn)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(isbn))
                return Error<BookInfoDto>("ISBN is required", StatusCodes.Status400BadRequest);

            var book = await _bookSearchService.GetBookByIsbnAsync(isbn);
            
            if (book == null)
                return Error<BookInfoDto>("Book not found with the specified ISBN", StatusCodes.Status404NotFound);

            _logger.LogInformation("Book found by ISBN '{ISBN}': {Title} by {Author}", isbn, book.Title, book.Author);

            return Success(book.ToDto(), "Book found successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching book by ISBN '{ISBN}'", isbn);
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Search books with detailed parameters
    /// </summary>
    [HttpPost("search")]
    [ProducesResponseType(typeof(ApiResponse<List<BookInfoDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<List<BookInfoDto>>>> SearchBooksDetailed([FromBody] BookSearchRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return ValidationError<List<BookInfoDto>>(errors);
            }

            var books = await _bookSearchService.SearchBooksAsync(request.Query, request.MaxResults);
            var bookDtos = books.ToDto();

            _logger.LogInformation("Detailed book search completed for query '{Query}', found {Count} results", 
                request.Query, bookDtos.Count);

            return Success(bookDtos, $"Found {bookDtos.Count} books matching '{request.Query}'");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in detailed book search with query '{Query}'", request.Query);
            return HandleException(ex);
        }
    }
}
