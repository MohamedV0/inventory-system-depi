using System.Threading.Tasks;
using FluentValidation;
using InventoryManagementSystem.Models.ViewModels;
using InventoryManagementSystem.Data.Repositories;
using InventoryManagementSystem.Models.Entities;
using InventoryManagementSystem.Data.Repositories.Interfaces;

namespace InventoryManagementSystem.Models.Validation
{
    public class CategoryValidator : AbstractValidator<CategoryViewModel>
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryValidator(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category name is required")
                .MaximumLength(100).WithMessage("Category name cannot exceed 100 characters")
                .MustAsync(BeUniqueNameExceptSelf).WithMessage("Category name already exists");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");
                
            // Add a rule to ensure at least one field has changed during update
            RuleFor(x => x)
                .MustAsync(AtLeastOneFieldChanged)
                .WithMessage("No changes detected. Please modify at least one field before saving.")
                .When(x => x.Id > 0); // Only apply this rule for updates, not for new categories
        }

        private async Task<bool> BeUniqueNameExceptSelf(CategoryViewModel model, string name, CancellationToken cancellationToken)
        {
            // If this is an existing category (edit operation)
            if (model.Id > 0)
            {
                // Get the category to see if the name has changed
                var existingCategory = await _categoryRepository.GetByIdAsync(model.Id, cancellationToken);
                if (existingCategory.IsSuccess && 
                    existingCategory.Value.Name.Trim().Equals(name.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    // Name hasn't changed, so it's valid
                    return true;
                }
            }
            
            // Check if another category has this name
            var result = await _categoryRepository.CategoryNameExistsAsync(name);
            return !result.Value;
        }
        
        private async Task<bool> AtLeastOneFieldChanged(CategoryViewModel model, CancellationToken cancellationToken)
        {
            // Only applicable for updates
            if (model.Id <= 0)
                return true;
                
            // Get the existing category to compare
            var existingResult = await _categoryRepository.GetByIdAsync(model.Id, cancellationToken);
            if (!existingResult.IsSuccess)
                return false;
                
            var existing = existingResult.Value;
            
            // Check if any field has changed
            if (!existing.Name.Trim().Equals(model.Name.Trim(), StringComparison.OrdinalIgnoreCase))
                return true; // Name changed
                
            bool descriptionChanged = false;
            if (string.IsNullOrEmpty(existing.Description) && !string.IsNullOrEmpty(model.Description))
                descriptionChanged = true;
            else if (!string.IsNullOrEmpty(existing.Description) && string.IsNullOrEmpty(model.Description))
                descriptionChanged = true;
            else if (!string.IsNullOrEmpty(existing.Description) && !string.IsNullOrEmpty(model.Description) && 
                    !existing.Description.Trim().Equals(model.Description.Trim(), StringComparison.OrdinalIgnoreCase))
                descriptionChanged = true;
                
            if (descriptionChanged)
                return true; // Description changed
                
            if (existing.IsActive != model.IsActive)
                return true; // Status changed
                
            // No changes detected
            return false;
        }

        public (bool IsValid, List<string> Errors) ValidateCategory(Category category)
        {
            var errors = new List<string>();

            // Name validation
            if (string.IsNullOrWhiteSpace(category.Name))
            {
                errors.Add("Category name is required.");
            }
            else if (category.Name.Length > 100)
            {
                errors.Add("Category name cannot exceed 100 characters.");
            }

            // Description validation
            if (!string.IsNullOrWhiteSpace(category.Description) && category.Description.Length > 500)
            {
                errors.Add("Category description cannot exceed 500 characters.");
            }

            return (errors.Count == 0, errors);
        }
    }
} 