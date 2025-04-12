using System;
using System.Net;

namespace InventoryManagementSystem.Models.Exceptions
{
    /// <summary>
    /// Exception thrown when a user is authenticated but does not have permission for a specific action
    /// </summary>
    public class ForbiddenException : AppException
    {
        /// <summary>
        /// Creates a new ForbiddenException with a default message
        /// </summary>
        public ForbiddenException()
            : base("You do not have permission to access this resource", (int)HttpStatusCode.Forbidden, "FORBIDDEN")
        {
        }

        /// <summary>
        /// Creates a new ForbiddenException with the specified message
        /// </summary>
        /// <param name="message">Error message</param>
        public ForbiddenException(string message)
            : base(message, (int)HttpStatusCode.Forbidden, "FORBIDDEN")
        {
        }

        /// <summary>
        /// Creates a new ForbiddenException for a specific resource and action
        /// </summary>
        /// <param name="resource">The resource being accessed</param>
        /// <param name="action">The action being attempted</param>
        public ForbiddenException(string resource, string action)
            : base($"You do not have permission to {action} the {resource}", (int)HttpStatusCode.Forbidden, "FORBIDDEN")
        {
        }

        /// <summary>
        /// Creates a new ForbiddenException with the specified message and inner exception
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="innerException">Inner exception</param>
        public ForbiddenException(string message, Exception innerException)
            : base(message, innerException, (int)HttpStatusCode.Forbidden, "FORBIDDEN")
        {
        }
    }
} 