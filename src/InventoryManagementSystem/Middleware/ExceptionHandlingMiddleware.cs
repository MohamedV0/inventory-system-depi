using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using InventoryManagementSystem.Models.Exceptions;
using InventoryManagementSystem.Helpers;

namespace InventoryManagementSystem.Middleware
{
    /// <summary>
    /// Middleware that catches unhandled exceptions at the application level.
    /// This acts as a global safety net for exceptions not caught by filters.
    /// Use this for:
    /// - Unhandled exceptions anywhere in the pipeline
    /// - Exceptions occurring outside of controller actions
    /// - Providing a last-resort exception handling mechanism
    /// </summary>
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
            catch (Exception ex)
            {
                // Get or create a correlation ID for tracking the request
                string correlationId = LoggingHelper.GetOrCreateCorrelationId(context);
                
                // Log the exception with correlation ID
                _logger.LogError(ex, 
                    "[CID:{CorrelationId}] Unhandled exception in global middleware: {Message}",
                    correlationId, ex.Message);

                // Handle the exception and generate a response
                await HandleExceptionAsync(context, ex, correlationId);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception, string correlationId)
        {
            // Set default status code and error details
            int statusCode = (int)HttpStatusCode.InternalServerError;
            string message = "An unexpected error occurred. Please try again later.";
            string errorCode = "INTERNAL_SERVER_ERROR";
            
            // Determine the appropriate status code and message based on the exception type
            if (exception is AppException appException)
            {
                statusCode = (int)appException.StatusCode;
                message = appException.Message;
                errorCode = appException.ErrorCode;
            }
            else if (exception is UnauthorizedAccessException)
            {
                statusCode = (int)HttpStatusCode.Unauthorized;
                message = "You are not authorized to perform this action.";
                errorCode = "UNAUTHORIZED";
            }
            else if (exception is DbUpdateConcurrencyException)
            {
                statusCode = (int)HttpStatusCode.Conflict;
                message = "The data you're trying to modify has been changed by another user.";
                errorCode = "CONCURRENCY_FAILURE";
            }
            else if (exception is DbUpdateException)
            {
                statusCode = (int)HttpStatusCode.BadRequest;
                message = "A database error occurred while saving changes.";
                errorCode = "DATABASE_ERROR";
            }

            // Set the response status code
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            // Create error response object
            var response = new ErrorResponse
            {
                StatusCode = statusCode,
                Message = message,
                ErrorCode = errorCode,
                CorrelationId = correlationId,
                TraceId = context.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            };

            // Include additional details in development environment
            if (_environment.IsDevelopment())
            {
                response.Details = exception.ToString();
                
                // Add request details in development mode only
                response.RequestPath = context.Request.Path;
                response.RequestMethod = context.Request.Method;
            }

            // Serialize the response to JSON
            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            // Write the response
            await context.Response.WriteAsync(jsonResponse);
        }
    }

    /// <summary>
    /// Standard error response format for all API responses
    /// </summary>
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;
        public string CorrelationId { get; set; } = string.Empty;
        public string TraceId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? Details { get; set; }
        public string? RequestPath { get; set; }
        public string? RequestMethod { get; set; }
    }
} 