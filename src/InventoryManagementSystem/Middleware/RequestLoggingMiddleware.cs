using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestPath = context.Request?.Path.Value ?? "unknown";
        var requestMethod = context.Request?.Method ?? "unknown";
        var userId = context.User?.Identity?.Name ?? "anonymous";

        try
        {
            _logger.LogInformation(
                "Starting {Method} request to {Path} by user {UserId}",
                requestMethod,
                requestPath,
                userId);

            await _next(context);

            stopwatch.Stop();

            _logger.LogInformation(
                "Completed {Method} {Path} with {StatusCode} in {ElapsedMs}ms",
                requestMethod,
                requestPath,
                context.Response?.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
        catch
        {
            stopwatch.Stop();
            _logger.LogWarning(
                "Failed {Method} {Path} with {StatusCode} in {ElapsedMs}ms",
                requestMethod,
                requestPath,
                context.Response?.StatusCode,
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
} 