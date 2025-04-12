using InventoryManagementSystem.Models.Entities;
using System.Collections.Generic;
using FluentValidation;
using InventoryManagementSystem.Models.ViewModels;
using System.Threading;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Models.Validation
{
    /// <summary>
    /// Provides validation logic for ProductSupplier entities
    /// </summary>
    public class ProductSupplierValidator : AbstractValidator<ProductSupplierViewModel>
    {
        public ProductSupplierValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("Valid product ID is required");
                
            RuleFor(x => x.SupplierId)
                .GreaterThan(0).WithMessage("Valid supplier ID is required");
                
            RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Unit price must be greater than or equal to 0");
                
            RuleFor(x => x.LeadTimeDays)
                .GreaterThanOrEqualTo(0).WithMessage("Lead time must be greater than or equal to 0");
                
            RuleFor(x => x.MinimumOrderQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Minimum order quantity must be greater than or equal to 0");
        }

        public new async Task<FluentValidation.Results.ValidationResult> ValidateAsync(ProductSupplierViewModel productSupplier, CancellationToken cancellationToken = default)
        {
            return await base.ValidateAsync(productSupplier, cancellationToken);
        }
        
        public async Task<FluentValidation.Results.ValidationResult> ValidateAsync(CreateProductSupplierViewModel productSupplier, CancellationToken cancellationToken = default)
        {
            var validator = new InlineValidator<CreateProductSupplierViewModel>();
            
            validator.RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("Valid product ID is required");
                
            validator.RuleFor(x => x.SupplierId)
                .GreaterThan(0).WithMessage("Valid supplier ID is required");
                
            validator.RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Unit price must be greater than or equal to 0");
                
            validator.RuleFor(x => x.LeadTimeDays)
                .GreaterThanOrEqualTo(0).WithMessage("Lead time must be greater than or equal to 0");
                
            validator.RuleFor(x => x.MinimumOrderQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Minimum order quantity must be greater than or equal to 0");
                
            return await validator.ValidateAsync(productSupplier, cancellationToken);
        }
        
        public async Task<FluentValidation.Results.ValidationResult> ValidateAsync(UpdateProductSupplierViewModel productSupplier, CancellationToken cancellationToken = default)
        {
            var validator = new InlineValidator<UpdateProductSupplierViewModel>();
            
            validator.RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("Valid product ID is required");
                
            validator.RuleFor(x => x.SupplierId)
                .GreaterThan(0).WithMessage("Valid supplier ID is required");
                
            validator.RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Unit price must be greater than or equal to 0");
                
            validator.RuleFor(x => x.LeadTimeDays)
                .GreaterThanOrEqualTo(0).WithMessage("Lead time must be greater than or equal to 0");
                
            validator.RuleFor(x => x.MinimumOrderQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Minimum order quantity must be greater than or equal to 0");
                
            return await validator.ValidateAsync(productSupplier, cancellationToken);
        }

        /// <summary>
        /// Validates a ProductSupplier entity
        /// </summary>
        /// <param name="productSupplier">The product-supplier relationship to validate</param>
        /// <returns>A tuple containing a boolean indicating if the relationship is valid and a list of error messages</returns>
        public (bool isValid, List<string> errors) ValidateProductSupplier(ProductSupplier productSupplier)
        {
            var errors = new List<string>();

            if (productSupplier == null)
            {
                errors.Add("Product-Supplier relationship cannot be null");
                return (false, errors);
            }

            if (productSupplier.ProductId <= 0)
                errors.Add("Valid product ID is required");

            if (productSupplier.SupplierId <= 0)
                errors.Add("Valid supplier ID is required");

            if (productSupplier.SupplierPrice < 0)
                errors.Add("Supplier price must be greater than or equal to 0");

            if (productSupplier.SupplierSKU?.Length > 50)
                errors.Add("Supplier SKU cannot exceed 50 characters");

            if (productSupplier.LeadTimeDays < 0)
                errors.Add("Lead time days must be greater than or equal to 0");

            return (!errors.Any(), errors);
        }

        /// <summary>
        /// Validates a list of product-supplier relationships
        /// </summary>
        /// <param name="productSuppliers">The relationships to validate</param>
        /// <returns>A dictionary containing validation results for each relationship</returns>
        public Dictionary<ProductSupplier, List<string>> ValidateProductSuppliers(IEnumerable<ProductSupplier> productSuppliers)
        {
            var validationResults = new Dictionary<ProductSupplier, List<string>>();

            if (productSuppliers == null)
                return validationResults;

            foreach (var productSupplier in productSuppliers)
            {
                var (_, errors) = ValidateProductSupplier(productSupplier);
                if (errors.Any())
                    validationResults.Add(productSupplier, errors);
            }

            return validationResults;
        }

        /// <summary>
        /// Validates the pricing relationship between product and supplier
        /// </summary>
        /// <param name="productSupplier">The relationship to validate</param>
        /// <param name="product">The associated product</param>
        /// <returns>A tuple containing a boolean indicating if the pricing is valid and a list of warnings</returns>
        public (bool isValid, List<string> warnings) ValidatePricingRelationship(ProductSupplier productSupplier, Product product)
        {
            var warnings = new List<string>();

            if (productSupplier == null || product == null)
                return (false, warnings);

            if (productSupplier.SupplierPrice > product.Price)
                warnings.Add("Supplier price is higher than product selling price");

            var margin = ((product.Price - productSupplier.SupplierPrice) / productSupplier.SupplierPrice) * 100;
            if (margin < 20)
                warnings.Add($"Low profit margin of {margin:F2}% with this supplier");

            if (productSupplier.IsPreferred && margin < 25)
                warnings.Add("Low profit margin for a preferred supplier");

            return (!warnings.Any(), warnings);
        }

        /// <summary>
        /// Validates the lead time for a product-supplier relationship
        /// </summary>
        /// <param name="productSupplier">The relationship to validate</param>
        /// <returns>A tuple containing a boolean indicating if the lead time is acceptable and a list of warnings</returns>
        public (bool isValid, List<string> warnings) ValidateLeadTime(ProductSupplier productSupplier)
        {
            var warnings = new List<string>();

            if (productSupplier == null)
                return (false, warnings);

            if (productSupplier.LeadTimeDays > 30)
                warnings.Add($"Long lead time of {productSupplier.LeadTimeDays} days");

            if (productSupplier.IsPreferred && productSupplier.LeadTimeDays > 14)
                warnings.Add("Long lead time for a preferred supplier");

            return (!warnings.Any(), warnings);
        }
    }
} 