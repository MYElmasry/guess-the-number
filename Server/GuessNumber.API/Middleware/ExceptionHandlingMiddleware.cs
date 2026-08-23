using GuessNumber.Application.Exceptions;
using GuessNumber.API.Models;
using System.Net;
using System.Text.Json;

namespace GuessNumber.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AppException ex)
        {
            await WriteErrorAsync(context, ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            var message = _environment.IsDevelopment()
                ? ex.Message
                : "An unexpected error occurred.";
            await WriteErrorAsync(context, (int)HttpStatusCode.InternalServerError, message);
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var payload = new ApiErrorResponse(message);
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
