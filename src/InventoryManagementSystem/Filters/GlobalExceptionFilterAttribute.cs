using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Net;
using System.Collections.Generic;
using InventoryManagementSystem.Models.Exceptions;
using InventoryManagementSystem.Helpers;
using FluentValidation;
using ValidationException = FluentValidation.ValidationException;

namespace InventoryManagementSystem.Filters
{
    /// <summary>
    /// Global exception filter that handles exceptions specifically from controller actions.
    /// This focuses on controller/action level exceptions and provides specialized handling for:
    /// - Model validation errors
    /// - Controller-specific exceptions
    /// - Formatting responses appropriate for API vs MVC requests
    /// 
    /// Note: Global unhandled exceptions are caught by ExceptionHandlingMiddleware
    /// </summary>
    public class GlobalExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private readonly ILogger<GlobalExceptionFilterAttribute> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly IModelMetadataProvider _modelMetadataProvider;

        public GlobalExceptionFilterAttribute(
            ILogger<GlobalExceptionFilterAttribute> logger,
            IWebHostEnvironment environment,
            IModelMetadataProvider modelMetadataProvider)
        {
            _logger = logger;
            _environment = environment;
            _modelMetadataProvider = modelMetadataProvider;
        }

        public override void OnException(ExceptionContext context)
        {
            // If the exception is already handled, don't process it again
            if (context.ExceptionHandled)
                return;
                
            // Get correlation ID for tracking
            string correlationId = LoggingHelper.GetOrCreateCorrelationId(context.HttpContext);
            
            // Log the exception with correlation ID
            _logger.LogError(exception: context.Exception, 
                message: "[CID:{CorrelationId}] Controller exception in {Controller}.{Action}: {Message}",
                correlationId, 
                context.RouteData.Values["controller"],
                context.RouteData.Values["action"],
                context.Exception.Message);

            // Handle the exception differently depending on if it's an API or MVC request
            if (IsApiRequest(context))
            {
                HandleApiException(context, correlationId);
            }
            else
            {
                HandleMvcException(context, correlationId);
            }

            // Mark as handled so the middleware doesn't process it again
            context.ExceptionHandled = true;
        }

        private bool IsApiRequest(ExceptionContext context)
        {
            return context.HttpContext.Request.Path.StartsWithSegments("/api") || 
                   context.HttpContext.Request.Headers["Accept"].Any(h => h?.Contains("application/json") == true);
        }

        private void HandleApiException(ExceptionContext context, string correlationId)
        {
            // The HTTP status code to use
            int statusCode = (int)HttpStatusCode.InternalServerError;
            string errorCode = "INTERNAL_SERVER_ERROR";
            string message = "An unexpected error occurred. Please try again later.";
            Dictionary<string, string[]>? validationErrors = null;
            
            // Get more information based on exception type
            if (context.Exception is AppException appEx)
            {
                statusCode = (int)appEx.StatusCode;
                message = appEx.Message;
                errorCode = appEx.ErrorCode;
            }
            else if (context.Exception is UnauthorizedAccessException)
            {
                statusCode = (int)HttpStatusCode.Unauthorized;
                message = "You are not authorized to perform this action.";
                errorCode = "UNAUTHORIZED";
            }
            else if (context.Exception is NotFoundException)
            {
                statusCode = (int)HttpStatusCode.NotFound;
                message = context.Exception.Message;
                errorCode = "NOT_FOUND";
            }
            else if (context.Exception is ValidationException validationEx)
            {
                statusCode = (int)HttpStatusCode.BadRequest;
                message = "One or more validation errors occurred.";
                errorCode = "VALIDATION_ERROR";
                
                // Create validation errors dictionary from FluentValidation errors
                validationErrors = new Dictionary<string, string[]>();
                
                foreach (var error in validationEx.Errors)
                {
                    string propertyName = error.PropertyName;
                    
                    if (!validationErrors.ContainsKey(propertyName))
                    {
                        validationErrors[propertyName] = new string[] { error.ErrorMessage };
                    }
                    else
                    {
                        var currentErrors = validationErrors[propertyName].ToList();
                        currentErrors.Add(error.ErrorMessage);
                        validationErrors[propertyName] = currentErrors.ToArray();
                    }
                }
            }
            else if (context.Exception is Models.Exceptions.ValidationException appValidationEx)
            {
                statusCode = (int)HttpStatusCode.BadRequest;
                message = appValidationEx.Message;
                errorCode = appValidationEx.ErrorCode;
            }
            else if (context.Exception is ForbiddenException)
            {
                statusCode = (int)HttpStatusCode.Forbidden;
                message = context.Exception.Message;
                errorCode = "FORBIDDEN";
            }
            else if (context.Exception is DbUpdateConcurrencyException)
            {
                statusCode = (int)HttpStatusCode.Conflict;
                message = "The data you're trying to modify has been changed by another user.";
                errorCode = "CONCURRENCY_FAILURE";
            }
            else if (context.Exception is DbUpdateException)
            {
                statusCode = (int)HttpStatusCode.BadRequest;
                message = "A database error occurred while saving changes.";
                errorCode = "DATABASE_ERROR";
            }

            // Create standardized response
            object response;
            
            if (validationErrors != null)
            {
                response = new
                {
                    StatusCode = statusCode,
                    Message = message,
                    ErrorCode = errorCode,
                    CorrelationId = correlationId,
                    Timestamp = DateTime.UtcNow,
                    ValidationErrors = validationErrors,
                    Details = _environment.IsDevelopment() ? context.Exception.ToString() : null
                };
            }
            else
            {
                response = new
                {
                    StatusCode = statusCode,
                    Message = message,
                    ErrorCode = errorCode,
                    CorrelationId = correlationId,
                    Timestamp = DateTime.UtcNow,
                    Details = _environment.IsDevelopment() ? context.Exception.ToString() : null
                };
            }

            context.Result = new JsonResult(response)
            {
                StatusCode = statusCode
            };
        }

        private void HandleMvcException(ExceptionContext context, string correlationId)
        {
            var viewName = "Error";
            int statusCode = (int)HttpStatusCode.InternalServerError;
            string message = "An unexpected error occurred.";
            
            // Customize based on exception type
            if (context.Exception is NotFoundException)
            {
                viewName = "NotFound";
                statusCode = (int)HttpStatusCode.NotFound;
                message = context.Exception.Message;
            }
            else if (context.Exception is ValidationException || 
                    context.Exception is Models.Exceptions.ValidationException)
            {
                viewName = "ValidationError";
                statusCode = (int)HttpStatusCode.BadRequest;
                message = "One or more validation errors occurred.";
            }
            else if (context.Exception is UnauthorizedAccessException)
            {
                viewName = "Unauthorized";
                statusCode = (int)HttpStatusCode.Unauthorized;
                message = "You are not authorized to perform this action.";
            }
            else if (context.Exception is ForbiddenException)
            {
                viewName = "Forbidden";
                statusCode = (int)HttpStatusCode.Forbidden;
                message = context.Exception.Message;
            }

            // Set status code
            context.HttpContext.Response.StatusCode = statusCode;

            // Create view model for error page
            var viewModel = new Models.ErrorViewModel
            {
                RequestId = context.HttpContext.TraceIdentifier,
                Message = message,
            };

            var result = new ViewResult
            {
                ViewName = viewName,
                ViewData = new ViewDataDictionary(_modelMetadataProvider, context.ModelState)
                {
                    Model = viewModel
                }
            };

            context.Result = result;
        }
    }

    /// <summary>
    /// Model class for error views - internal to this filter
    /// </summary>
    public class ErrorViewModel
    {
        public string RequestId { get; set; } = string.Empty;
        public string CorrelationId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool ShowStackTrace { get; set; }
        public string? StackTrace { get; set; }
        
        // These properties are automatically mapped to Models.ErrorViewModel to avoid conflicts
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
} 