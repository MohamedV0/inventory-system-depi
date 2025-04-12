using System;
using System.Net;

namespace InventoryManagementSystem.Models.Exceptions
{
    /// <summary>
    /// Exception thrown when a user is not authorized to perform an action
    /// </summary>
    public class UnauthorizedException : AppException
    {
        /// <summary>
        /// Creates a new UnauthorizedException with a default message
        /// </summary>
        public UnauthorizedException()
            : base("You are not authorized to perform this action", (int)HttpStatusCode.Unauthorized, "UNAUTHORIZED")
        {
        }

        /// <summary>
        /// Creates a new UnauthorizedException with the specified message
        /// </summary>
        /// <param name="message">Error message</param>
        public UnauthorizedException(string message)
            : base(message, (int)HttpStatusCode.Unauthorized, "UNAUTHORIZED")
        {
        }

        /// <summary>
        /// Creates a new UnauthorizedException with the specified message and inner exception
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="innerException">Inner exception</param>
        public UnauthorizedException(string message, Exception innerException)
            : base(message, innerException, (int)HttpStatusCode.Unauthorized, "UNAUTHORIZED")
        {
        }
    }
} 