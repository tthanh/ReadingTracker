using System.Net;
using System.Text.Json;
using ReadingTracker.Api.Models;
using ReadingTracker.Domain.Exceptions;

namespace ReadingTracker.Api.Middleware;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred while processing the request");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = new ApiResponse();

        switch (exception)
        {
            case UserBookNotFoundException:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response = ApiResponse.ErrorResult("The requested book was not found");
                break;

            case InvalidReadingOperationException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = ApiResponse.ErrorResult(exception.Message);
                break;

            case DomainException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = ApiResponse.ErrorResult(exception.Message);
                break;

            case ArgumentException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = ApiResponse.ErrorResult(exception.Message);
                break;

            case InvalidOperationException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = ApiResponse.ErrorResult(exception.Message);
                break;

            case UnauthorizedAccessException:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response = ApiResponse.ErrorResult("Unauthorized access");
                break;

            case TimeoutException:
                context.Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                response = ApiResponse.ErrorResult("The request timed out");
                break;

            case HttpRequestException:
                context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                response = ApiResponse.ErrorResult("External service is temporarily unavailable");
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response = ApiResponse.ErrorResult("An unexpected error occurred");
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}
