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
                .MustAsync(BeUniqueName).WithMessage("Category name already exists");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");
        }

        private async Task<bool> BeUniqueName(string name, CancellationToken cancellationToken)
        {
            var result = await _categoryRepository.CategoryNameExistsAsync(name);
            return !result.Value;
        }

        public new async Task<FluentValidation.Results.ValidationResult> ValidateAsync(CategoryViewModel category, CancellationToken cancellationToken = default)
        {
            return await base.ValidateAsync(category, cancellationToken);
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