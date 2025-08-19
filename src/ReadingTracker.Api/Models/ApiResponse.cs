using System.Net;

namespace ReadingTracker.Api.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> SuccessResult(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    public static ApiResponse<T> ErrorResult(string error)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Errors = new List<string> { error }
        };
    }

    public static ApiResponse<T> ErrorResult(List<string> errors)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Errors = errors
        };
    }
}

public class ApiResponse : ApiResponse<object>
{
    public new static ApiResponse Success(string? message = null)
    {
        return new ApiResponse
        {
            Message = message,
            Errors = new List<string>()
        };
    }

    public new static ApiResponse ErrorResult(string error)
    {
        return new ApiResponse
        {
            Message = null,
            Errors = new List<string> { error }
        };
    }

    public new static ApiResponse ErrorResult(List<string> errors)
    {
        return new ApiResponse
        {
            Message = null,
            Errors = errors
        };
    }
}

public class ValidationError
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class ValidationErrorResponse
{
    public string Title { get; set; } = "Validation failed";
    public int Status { get; set; } = (int)HttpStatusCode.BadRequest;
    public List<ValidationError> Errors { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
