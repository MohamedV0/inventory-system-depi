using System;
using System.Collections.Generic;

namespace InventoryManagementSystem.Models.Exceptions
{
    /// <summary>
    /// Base exception class for all application-specific exceptions
    /// </summary>
    public abstract class AppException : Exception
    {
        /// <summary>
        /// HTTP status code to return when this exception is thrown
        /// </summary>
        public int StatusCode { get; }

        /// <summary>
        /// Error code that uniquely identifies the error type
        /// </summary>
        public string ErrorCode { get; }

        /// <summary>
        /// Additional error details for debugging or client consumption
        /// </summary>
        public List<string> Details { get; }

        /// <summary>
        /// Timestamp when the exception occurred
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// Creates a new application exception with the specified message and status code
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="statusCode">HTTP status code</param>
        /// <param name="errorCode">Error code</param>
        /// <param name="details">Optional error details</param>
        protected AppException(string message, int statusCode, string errorCode, List<string>? details = null)
            : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
            Details = details ?? new List<string>();
            Timestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Creates a new application exception with the specified message, inner exception, and status code
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="innerException">Inner exception</param>
        /// <param name="statusCode">HTTP status code</param>
        /// <param name="errorCode">Error code</param>
        /// <param name="details">Optional error details</param>
        protected AppException(string message, Exception innerException, int statusCode, string errorCode, List<string>? details = null)
            : base(message, innerException)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
            Details = details ?? new List<string>();
            Timestamp = DateTime.UtcNow;
        }
    }
} 