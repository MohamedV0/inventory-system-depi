using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;

namespace InventoryManagementSystem.Models.Common
{
    /// <summary>
    /// Generic wrapper for operation results, providing a standardized way to handle success and error cases
    /// across all layers of the application.
    /// </summary>
    /// <typeparam name="T">The type of data being returned</typeparam>
    public class Result<T>
    {
        /// <summary>
        /// The data payload of the result
        /// </summary>
        public T? Value { get; private set; }

        /// <summary>
        /// Indicates whether the operation was successful
        /// </summary>
        [Required]
        public bool IsSuccess { get; private set; }

        /// <summary>
        /// A message describing the result of the operation
        /// </summary>
        [Required]
        [StringLength(500)]
        public string Message { get; private set; } = string.Empty;

        /// <summary>
        /// Optional error details when IsSuccess is false
        /// </summary>
        public List<string> Errors { get; private set; } = new();

        /// <summary>
        /// Appropriate HTTP status code for the result
        /// </summary>
        public int StatusCode { get; private set; } = (int)HttpStatusCode.OK;

        /// <summary>
        /// Timestamp of when the result was created
        /// </summary>
        public DateTime Timestamp { get; private set; } = DateTime.UtcNow;

        private Result(bool isSuccess, string message, T? value = default, List<string>? errors = null, int statusCode = (int)HttpStatusCode.OK)
        {
            IsSuccess = isSuccess;
            Message = message;
            Value = value;
            Errors = errors ?? new List<string>();
            StatusCode = statusCode;
        }

        /// <summary>
        /// Creates a successful result with the specified data and optional message
        /// </summary>
        public static Result<T> Success(T value, string message = "Operation completed successfully")
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value), "Success result must include a value");
            
            return new Result<T>(
                isSuccess: true, 
                message: message, 
                value: value, 
                statusCode: (int)HttpStatusCode.OK
            );
        }
        
        /// <summary>
        /// Creates a failure result with the specified message and optional error details
        /// </summary>
        public static Result<T> Failure(string message, List<string>? errors = null, int statusCode = (int)HttpStatusCode.BadRequest)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Error message cannot be empty", nameof(message));
            
            return new Result<T>(
                isSuccess: false, 
                message: message, 
                value: default, 
                errors: errors ?? new List<string> { message },
                statusCode: statusCode
            );
        }

        /// <summary>
        /// Creates a not found result with a standard message
        /// </summary>
        public static Result<T> NotFound(string entityName)
        {
            return Failure(
                message: $"{entityName} not found", 
                statusCode: (int)HttpStatusCode.NotFound
            );
        }

        /// <summary>
        /// Creates a validation error result
        /// </summary>
        public static Result<T> ValidationError(string message, List<string>? validationErrors = null)
        {
            var errorMessage = $"Validation error: {message}";
            return Failure(
                message: errorMessage, 
                errors: validationErrors,
                statusCode: (int)HttpStatusCode.BadRequest
            );
        }

        /// <summary>
        /// Creates a duplicate error result
        /// </summary>
        public static Result<T> DuplicateError(string entityName, string identifier)
        {
            return Failure(
                message: $"{entityName} with {identifier} already exists",
                statusCode: (int)HttpStatusCode.Conflict
            );
        }

        /// <summary>
        /// Creates an unauthorized result
        /// </summary>
        public static Result<T> Unauthorized(string message = "Unauthorized access")
        {
            return Failure(
                message: message,
                statusCode: (int)HttpStatusCode.Unauthorized
            );
        }

        /// <summary>
        /// Creates a conflict error result
        /// </summary>
        public static Result<T> Conflict(string message)
        {
            return Failure(
                message: $"Conflict: {message}",
                statusCode: (int)HttpStatusCode.Conflict
            );
        }

        /// <summary>
        /// Checks if the result has any errors
        /// </summary>
        public bool HasErrors() => !IsSuccess && Errors.Any();

        /// <summary>
        /// Checks if the result represents a not found condition
        /// </summary>
        public bool IsNotFound() => !IsSuccess && StatusCode == (int)HttpStatusCode.NotFound;

        /// <summary>
        /// Checks if the result represents a validation error
        /// </summary>
        public bool IsValidationError() => !IsSuccess && Message.StartsWith("Validation error:", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Gets a string representation of the result
        /// </summary>
        public override string ToString()
        {
            var status = IsSuccess ? "Success" : "Error";
            var statusCodeText = ((HttpStatusCode)StatusCode).ToString();
            var errorDetails = Errors.Any() ? $" Errors: {string.Join(", ", Errors)}" : string.Empty;
            return $"[{status}] {statusCodeText}: {Message}{errorDetails}";
        }
    }
} 