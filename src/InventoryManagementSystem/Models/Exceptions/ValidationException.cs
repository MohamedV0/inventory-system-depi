using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace InventoryManagementSystem.Models.Exceptions
{
    /// <summary>
    /// Exception thrown when validation errors occur
    /// </summary>
    public class ValidationException : AppException
    {
        /// <summary>
        /// Collection of validation errors
        /// </summary>
        public IReadOnlyList<ValidationError> ValidationErrors { get; }

        /// <summary>
        /// Creates a new ValidationException with the specified validation errors
        /// </summary>
        /// <param name="errors">Collection of validation errors</param>
        public ValidationException(IEnumerable<ValidationError> errors)
            : base("Validation failed", (int)HttpStatusCode.BadRequest, "VALIDATION_ERROR", 
                  errors.Select(e => e.ToString()).ToList())
        {
            ValidationErrors = errors.ToList().AsReadOnly();
        }

        /// <summary>
        /// Creates a new ValidationException with a single validation error
        /// </summary>
        /// <param name="error">Validation error message</param>
        public ValidationException(string error)
            : base("Validation failed", (int)HttpStatusCode.BadRequest, "VALIDATION_ERROR", 
                  new List<string> { error })
        {
            ValidationErrors = new List<ValidationError> 
            {
                new ValidationError(string.Empty, error)
            }.AsReadOnly();
        }

        /// <summary>
        /// Creates a new ValidationException with a property-specific validation error
        /// </summary>
        /// <param name="propertyName">Name of the property that failed validation</param>
        /// <param name="error">Validation error message</param>
        public ValidationException(string propertyName, string error)
            : base($"Validation failed for {propertyName}", (int)HttpStatusCode.BadRequest, "VALIDATION_ERROR", 
                  new List<string> { $"{propertyName}: {error}" })
        {
            ValidationErrors = new List<ValidationError> 
            {
                new ValidationError(propertyName, error)
            }.AsReadOnly();
        }

        /// <summary>
        /// Creates a new ValidationException with multiple errors and an inner exception
        /// </summary>
        /// <param name="errors">Collection of validation errors</param>
        /// <param name="innerException">Inner exception</param>
        public ValidationException(IEnumerable<ValidationError> errors, Exception innerException)
            : base("Validation failed", innerException, (int)HttpStatusCode.BadRequest, "VALIDATION_ERROR", 
                  errors.Select(e => e.ToString()).ToList())
        {
            ValidationErrors = errors.ToList().AsReadOnly();
        }
    }

    /// <summary>
    /// Represents a single validation error
    /// </summary>
    public class ValidationError
    {
        /// <summary>
        /// Name of the property that failed validation
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Validation error message
        /// </summary>
        public string ErrorMessage { get; }

        /// <summary>
        /// Creates a new validation error
        /// </summary>
        /// <param name="propertyName">Name of the property that failed validation</param>
        /// <param name="errorMessage">Validation error message</param>
        public ValidationError(string propertyName, string errorMessage)
        {
            PropertyName = propertyName;
            ErrorMessage = errorMessage;
        }

        /// <summary>
        /// Returns a string representation of the validation error
        /// </summary>
        public override string ToString()
        {
            return string.IsNullOrEmpty(PropertyName)
                ? ErrorMessage
                : $"{PropertyName}: {ErrorMessage}";
        }
    }
} 