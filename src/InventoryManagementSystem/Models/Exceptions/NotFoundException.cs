using System;
using System.Net;

namespace InventoryManagementSystem.Models.Exceptions
{
    /// <summary>
    /// Exception thrown when a requested resource is not found
    /// </summary>
    public class NotFoundException : AppException
    {
        /// <summary>
        /// Creates a new NotFoundException for the specified entity and key
        /// </summary>
        /// <param name="entityName">Name of the entity that wasn't found</param>
        /// <param name="key">Key value used in the lookup</param>
        public NotFoundException(string entityName, object key)
            : base($"{entityName} with id {key} not found", (int)HttpStatusCode.NotFound, "RESOURCE_NOT_FOUND")
        {
        }

        /// <summary>
        /// Creates a new NotFoundException with a custom message
        /// </summary>
        /// <param name="message">Custom error message</param>
        public NotFoundException(string message)
            : base(message, (int)HttpStatusCode.NotFound, "RESOURCE_NOT_FOUND")
        {
        }

        /// <summary>
        /// Creates a new NotFoundException with a custom message and inner exception
        /// </summary>
        /// <param name="message">Custom error message</param>
        /// <param name="innerException">Inner exception</param>
        public NotFoundException(string message, Exception innerException)
            : base(message, innerException, (int)HttpStatusCode.NotFound, "RESOURCE_NOT_FOUND")
        {
        }
    }
} 