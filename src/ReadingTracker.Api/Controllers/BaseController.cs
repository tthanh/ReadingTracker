using Microsoft.AspNetCore.Mvc;
using ReadingTracker.Api.Models;

namespace ReadingTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class BaseController : ControllerBase
{
    protected ActionResult<ApiResponse<T>> Success<T>(T data, string? message = null)
    {
        return Ok(ApiResponse<T>.SuccessResult(data, message));
    }

    protected ActionResult<ApiResponse> Success(string? message = null)
    {
        return Ok(ApiResponse.Success(message));
    }

    protected ActionResult<ApiResponse<T>> Error<T>(string error, int statusCode = 400)
    {
        return StatusCode(statusCode, ApiResponse<T>.ErrorResult(error));
    }

    protected ActionResult<ApiResponse> Error(string error, int statusCode = 400)
    {
        return StatusCode(statusCode, ApiResponse.ErrorResult(error));
    }

    protected ActionResult<ApiResponse<T>> ValidationError<T>(List<string> errors)
    {
        return BadRequest(ApiResponse<T>.ErrorResult(errors));
    }

    protected ActionResult<ApiResponse> ValidationError(List<string> errors)
    {
        return BadRequest(ApiResponse.ErrorResult(errors));
    }

    protected Guid GetCurrentUserId()
    {
        // In a real application, this would extract the user ID from JWT token or authentication context
        // For demo purposes, returning a hardcoded user ID
        return Guid.Parse("12345678-1234-1234-1234-123456789012");
    }

    protected ActionResult HandleException(Exception ex)
    {
        // Log the exception here
        return StatusCode(500, ApiResponse.ErrorResult("An unexpected error occurred"));
    }
}
