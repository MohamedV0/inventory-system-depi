using FluentValidation;
using InventoryManagementSystem.Models.Entities;
using InventoryManagementSystem.Models.ViewModels;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Models.Validation
{
    /// <summary>
    /// Provides validation logic for Product entities
    /// </summary>
    public class ProductValidator : AbstractValidator<ProductViewModel>
    {
        public ProductValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required")
                .MaximumLength(100).WithMessage("Product name cannot exceed 100 characters");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Product description cannot exceed 500 characters");

            RuleFor(x => x.SKU)
                .NotEmpty().WithMessage("SKU is required")
                .MaximumLength(50).WithMessage("SKU cannot exceed 50 characters");

            RuleFor(x => x.UnitOfMeasurement)
                .NotEmpty().WithMessage("Unit of measurement is required")
                .MaximumLength(20).WithMessage("Unit of measurement cannot exceed 20 characters");

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Price must be greater than or equal to 0");

            RuleFor(x => x.Cost)
                .GreaterThanOrEqualTo(0).WithMessage("Cost must be greater than or equal to 0");

            RuleFor(x => x.ReorderLevel)
                .GreaterThanOrEqualTo(0).WithMessage("Reorder level must be greater than or equal to 0");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Valid category ID is required");
        }

        public new async Task<FluentValidation.Results.ValidationResult> ValidateAsync(ProductViewModel product, CancellationToken cancellationToken = default)
        {
            return await base.ValidateAsync(product, cancellationToken);
        }

        /// <summary>
        /// Validates a Product entity
        /// </summary>
        /// <param name="product">The product to validate</param>
        /// <returns>A tuple containing a boolean indicating if the product is valid and a list of error messages</returns>
        public (bool isValid, List<string> errors) ValidateProduct(Product product)
        {
            var errors = new List<string>();

            if (product == null)
            {
                errors.Add("Product cannot be null");
                return (false, errors);
            }

            if (string.IsNullOrWhiteSpace(product.Name))
                errors.Add("Product name is required");
            else if (product.Name.Length > 100)
                errors.Add("Product name cannot exceed 100 characters");

            if (product.Description?.Length > 500)
                errors.Add("Product description cannot exceed 500 characters");

            if (string.IsNullOrWhiteSpace(product.SKU))
                errors.Add("SKU is required");
            else if (product.SKU.Length > 50)
                errors.Add("SKU cannot exceed 50 characters");

            if (string.IsNullOrWhiteSpace(product.UnitOfMeasurement))
                errors.Add("Unit of measurement is required");
            else if (product.UnitOfMeasurement.Length > 20)
                errors.Add("Unit of measurement cannot exceed 20 characters");

            if (product.Price < 0)
                errors.Add("Price must be greater than or equal to 0");

            if (product.Cost < 0)
                errors.Add("Cost must be greater than or equal to 0");

            if (product.ReorderLevel < 0)
                errors.Add("Reorder level must be greater than or equal to 0");

            if (product.CurrentStock < 0)
                errors.Add("Current stock must be greater than or equal to 0");

            if (product.CategoryId <= 0)
                errors.Add("Valid category ID is required");

            return (!errors.Any(), errors);
        }

        /// <summary>
        /// Validates a list of products
        /// </summary>
        /// <param name="products">The products to validate</param>
        /// <returns>A dictionary containing validation results for each product</returns>
        public Dictionary<Product, List<string>> ValidateProducts(IEnumerable<Product> products)
        {
            var validationResults = new Dictionary<Product, List<string>>();

            if (products == null)
                return validationResults;

            foreach (var product in products)
            {
                var (_, errors) = ValidateProduct(product);
                if (errors.Any())
                    validationResults.Add(product, errors);
            }

            return validationResults;
        }

        /// <summary>
        /// Validates product price and cost relationship
        /// </summary>
        /// <param name="product">The product to validate</param>
        /// <returns>A tuple containing a boolean indicating if the price-cost relationship is valid and a list of warnings</returns>
        public (bool isValid, List<string> warnings) ValidatePriceCostRelationship(Product product)
        {
            var warnings = new List<string>();

            if (product == null)
                return (false, warnings);

            if (product.Price < product.Cost)
                warnings.Add("Warning: Product price is less than cost");

            return (!warnings.Any(), warnings);
        }
    }
}