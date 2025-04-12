using System;
using System.Net;

namespace InventoryManagementSystem.Models.Exceptions
{
    /// <summary>
    /// Exception thrown when a conflict occurs, such as a duplicate resource
    /// </summary>
    public class ConflictException : AppException
    {
        /// <summary>
        /// Creates a new ConflictException with the specified message
        /// </summary>
        /// <param name="message">Error message</param>
        public ConflictException(string message)
            : base(message, (int)HttpStatusCode.Conflict, "CONFLICT")
        {
        }

        /// <summary>
        /// Creates a new ConflictException for a duplicate entity
        /// </summary>
        /// <param name="entityName">Name of the entity</param>
        /// <param name="key">Key or identifier that caused the conflict</param>
        public ConflictException(string entityName, string key)
            : base($"{entityName} with identifier '{key}' already exists", (int)HttpStatusCode.Conflict, "DUPLICATE_ENTITY")
        {
        }

        /// <summary>
        /// Creates a new ConflictException with the specified message and inner exception
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="innerException">Inner exception</param>
        public ConflictException(string message, Exception innerException)
            : base(message, innerException, (int)HttpStatusCode.Conflict, "CONFLICT")
        {
        }
    }
} 