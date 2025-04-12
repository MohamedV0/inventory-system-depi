using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Helpers
{
    /// <summary>
    /// Helper class for standardized logging across the application.
    /// 
    /// This class provides common logging functionality for both the ExceptionHandlingMiddleware
    /// and the GlobalExceptionFilterAttribute to ensure consistent log formatting and context.
    /// </summary>
    public static class LoggingHelper
    {
        /// <summary>
        /// Logs an informational message with correlation ID and user context
        /// </summary>
        public static void LogInfo<T>(
            this ILogger<T> logger,
            HttpContext? httpContext,
            string message,
            params object[] args)
        {
            LogWithContext(logger, LogLevel.Information, httpContext, null, message, args);
        }

        /// <summary>
        /// Logs a warning message with correlation ID and user context
        /// </summary>
        public static void LogWarning<T>(
            this ILogger<T> logger,
            HttpContext? httpContext,
            string message,
            params object[] args)
        {
            LogWithContext(logger, LogLevel.Warning, httpContext, null, message, args);
        }

        /// <summary>
        /// Logs an error message with correlation ID, user context, and exception details
        /// </summary>
        public static void LogError<T>(
            this ILogger<T> logger,
            HttpContext? httpContext,
            Exception? exception,
            string message,
            params object[] args)
        {
            LogWithContext(logger, LogLevel.Error, httpContext, exception, message, args);
        }

        /// <summary>
        /// Logs a critical error message with correlation ID, user context, and exception details
        /// </summary>
        public static void LogCritical<T>(
            this ILogger<T> logger,
            HttpContext? httpContext,
            Exception? exception,
            string message,
            params object[] args)
        {
            LogWithContext(logger, LogLevel.Critical, httpContext, exception, message, args);
        }

        /// <summary>
        /// Helper method to log messages with standard context information
        /// </summary>
        private static void LogWithContext<T>(
            ILogger<T> logger,
            LogLevel logLevel,
            HttpContext? httpContext,
            Exception? exception,
            string message,
            params object[] args)
        {
            string correlationId = "not_available";
            string userId = "anonymous";
            string requestPath = "no_http_context";

            if (httpContext != null)
            {
                // Get correlation ID from headers or use TraceIdentifier
                correlationId = httpContext.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? httpContext.TraceIdentifier;
                
                // Get user ID if available
                userId = httpContext.User?.Identity?.Name ?? "anonymous";
                
                // Get request path
                requestPath = httpContext.Request.Path.Value ?? "unknown";
            }

            // Create enhanced message with context
            string enhancedMessage = $"[CID:{correlationId}] [User:{userId}] [Path:{requestPath}] {message}";

            // Log with appropriate log level
            switch (logLevel)
            {
                case LogLevel.Information:
                    logger.LogInformation(enhancedMessage, args);
                    break;
                case LogLevel.Warning:
                    logger.LogWarning(enhancedMessage, args);
                    break;
                case LogLevel.Error:
                    logger.LogError(exception, enhancedMessage, args);
                    break;
                case LogLevel.Critical:
                    logger.LogCritical(exception, enhancedMessage, args);
                    break;
                default:
                    logger.Log(logLevel, exception, enhancedMessage, args);
                    break;
            }
        }

        /// <summary>
        /// Gets or creates a correlation ID for a request.
        /// This is used by both the middleware and filter to ensure consistent tracking.
        /// </summary>
        public static string GetOrCreateCorrelationId(HttpContext context)
        {
            // Check if there's a correlation ID in the request headers
            string correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? string.Empty;
            
            // If not, create a new one using the TraceIdentifier
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = context.TraceIdentifier;
            }
            
            // Ensure the correlation ID is available to the response
            context.Response.Headers["X-Correlation-ID"] = correlationId;
            
            return correlationId;
        }
    }
} 