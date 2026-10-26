using System.Net;
using System.Text.Json;

namespace Shop.Presentation.Middleware;

// Bat loi tap trung -> tra JSON voi HTTP status phu hop cho tung loai loi.
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            _logger.LogError(ex, "Co ngoai le xay ra: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var statusCode = exception switch
        {
            KeyNotFoundException => HttpStatusCode.NotFound,               // 404
            ArgumentException or InvalidOperationException => HttpStatusCode.BadRequest, // 400
            _ => HttpStatusCode.InternalServerError                        // 500
        };

        context.Response.StatusCode = (int)statusCode;

        var payload = JsonSerializer.Serialize(new
        {
            status = context.Response.StatusCode,
            error = statusCode.ToString(),
            message = exception.Message
        });

        return context.Response.WriteAsync(payload);
    }
}
