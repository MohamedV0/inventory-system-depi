using System;
using System.Collections.Generic;
using System.Net;

namespace InventoryManagementSystem.Models.Exceptions
{
    /// <summary>
    /// Exception thrown when a bad request is made
    /// </summary>
    public class BadRequestException : AppException
    {
        /// <summary>
        /// Creates a new BadRequestException with the specified message
        /// </summary>
        /// <param name="message">Error message</param>
        public BadRequestException(string message)
            : base(message, (int)HttpStatusCode.BadRequest, "BAD_REQUEST")
        {
        }

        /// <summary>
        /// Creates a new BadRequestException with the specified message and details
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="details">Error details</param>
        public BadRequestException(string message, List<string> details)
            : base(message, (int)HttpStatusCode.BadRequest, "BAD_REQUEST", details)
        {
        }

        /// <summary>
        /// Creates a new BadRequestException with the specified message and inner exception
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="innerException">Inner exception</param>
        public BadRequestException(string message, Exception innerException)
            : base(message, innerException, (int)HttpStatusCode.BadRequest, "BAD_REQUEST")
        {
        }
    }
} 