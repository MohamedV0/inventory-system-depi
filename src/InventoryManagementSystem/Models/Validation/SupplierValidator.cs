using InventoryManagementSystem.Models.Entities;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FluentValidation;
using InventoryManagementSystem.Models.ViewModels;
using System.Threading;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Models.Validation
{
    /// <summary>
    /// Provides validation logic for Supplier entities
    /// </summary>
    public class SupplierValidator : AbstractValidator<SupplierViewModel>
    {
        private readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
        private readonly Regex PhoneRegex = new(@"^\+?[\d\s-()]{10,}$", RegexOptions.Compiled);
        private readonly Regex UrlRegex = new(@"^https?:\/\/[\w\-]+(\.[\w\-]+)+[/#?]?.*$", RegexOptions.Compiled);

        public SupplierValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Supplier name is required")
                .MaximumLength(100).WithMessage("Supplier name cannot exceed 100 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");

            RuleFor(x => x.Phone)
                .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters");

            RuleFor(x => x.Address)
                .MaximumLength(200).WithMessage("Address cannot exceed 200 characters");

            RuleFor(x => x.ContactPerson)
                .MaximumLength(100).WithMessage("Contact person name cannot exceed 100 characters");
        }

        /// <summary>
        /// Validates a Supplier entity
        /// </summary>
        /// <param name="supplier">The supplier to validate</param>
        /// <returns>A tuple containing a boolean indicating if the supplier is valid and a list of error messages</returns>
        public (bool isValid, List<string> errors) ValidateSupplier(Supplier supplier)
        {
            var errors = new List<string>();

            if (supplier == null)
            {
                errors.Add("Supplier cannot be null");
                return (false, errors);
            }

            if (string.IsNullOrWhiteSpace(supplier.Name))
                errors.Add("Supplier name is required");
            else if (supplier.Name.Length > 100)
                errors.Add("Supplier name cannot exceed 100 characters");

            if (string.IsNullOrWhiteSpace(supplier.ContactPerson))
                errors.Add("Contact person is required");
            else if (supplier.ContactPerson.Length > 100)
                errors.Add("Contact person name cannot exceed 100 characters");

            if (string.IsNullOrWhiteSpace(supplier.Email))
                errors.Add("Email is required");
            else if (supplier.Email.Length > 100)
                errors.Add("Email cannot exceed 100 characters");
            else if (!EmailRegex.IsMatch(supplier.Email))
                errors.Add("Invalid email format");

            if (string.IsNullOrWhiteSpace(supplier.Phone))
                errors.Add("Phone number is required");
            else if (supplier.Phone.Length > 20)
                errors.Add("Phone number cannot exceed 20 characters");
            else if (!PhoneRegex.IsMatch(supplier.Phone))
                errors.Add("Invalid phone number format");

            if (string.IsNullOrWhiteSpace(supplier.Address))
                errors.Add("Address is required");
            else if (supplier.Address.Length > 200)
                errors.Add("Address cannot exceed 200 characters");

            if (!string.IsNullOrWhiteSpace(supplier.Website))
            {
                if (supplier.Website.Length > 100)
                    errors.Add("Website URL cannot exceed 100 characters");
                else if (!UrlRegex.IsMatch(supplier.Website))
                    errors.Add("Invalid website URL format");
            }

            if (supplier.Notes?.Length > 500)
                errors.Add("Notes cannot exceed 500 characters");

            return (!errors.Any(), errors);
        }

        /// <summary>
        /// Validates a list of suppliers
        /// </summary>
        /// <param name="suppliers">The suppliers to validate</param>
        /// <returns>A dictionary containing validation results for each supplier</returns>
        public Dictionary<Supplier, List<string>> ValidateSuppliers(IEnumerable<Supplier> suppliers)
        {
            var validationResults = new Dictionary<Supplier, List<string>>();

            if (suppliers == null)
                return validationResults;

            foreach (var supplier in suppliers)
            {
                var (_, errors) = ValidateSupplier(supplier);
                if (errors.Any())
                    validationResults.Add(supplier, errors);
            }

            return validationResults;
        }

        /// <summary>
        /// Validates supplier contact information
        /// </summary>
        /// <param name="supplier">The supplier to validate</param>
        /// <returns>A tuple containing a boolean indicating if the contact information is valid and a list of warnings</returns>
        public (bool isValid, List<string> warnings) ValidateContactInformation(Supplier supplier)
        {
            var warnings = new List<string>();

            if (supplier == null)
                return (false, warnings);

            if (string.IsNullOrWhiteSpace(supplier.Website))
                warnings.Add("No website provided for online verification");

            if (supplier.Notes == null)
                warnings.Add("No additional notes or remarks provided");

            return (!warnings.Any(), warnings);
        }

        public new async Task<FluentValidation.Results.ValidationResult> ValidateAsync(SupplierViewModel supplier, CancellationToken cancellationToken = default)
        {
            return await base.ValidateAsync(supplier, cancellationToken);
        }
    }
} 