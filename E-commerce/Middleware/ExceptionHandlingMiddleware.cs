using System.Net;
using System.Text.Json;
using E_commerce.Exceptions;

namespace E_commerce.Middleware;

public class ExceptionHandlingMiddleware
{
    // RequestDelegate represents the next middleware component in the HTTP pipeline.
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    // Invoked automatically for every incoming HTTP request passing through the middleware pipeline.
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Pass execution to the next middleware/controller
            await _next(context);
        }
        catch (AppException ex)
        {
            // Catch custom application exceptions (NotFoundException, BadRequestException, ConflictException, etc.)
            _logger.LogWarning(ex, "Handled application exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            // Catch unexpected runtime exceptions, preventing sensitive stack trace leaks in responses
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleExceptionAsync(context, (int)HttpStatusCode.InternalServerError,
                "An unexpected error occurred. Please try again later.");
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, int statusCode, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new
        {
            statusCode,
            message
        };

        var json = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(json);
    }
}