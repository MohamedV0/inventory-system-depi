using InventoryManagementSystem.Data.Context;
using InventoryManagementSystem.Models.Common;
using InventoryManagementSystem.Models.Entities;
using InventoryManagementSystem.Models.Validation;
using InventoryManagementSystem.Models.ViewModels;
using InventoryManagementSystem.Services.Interfaces;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using InventoryManagementSystem.Helpers;
using System.Threading;
using AutoMapper;
using InventoryManagementSystem.Extensions;

namespace InventoryManagementSystem.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CategoryService> _logger;
        private readonly CategoryValidator _validator;
        private readonly IMapper _mapper;
        private const int MaxPageSize = 100;

        public CategoryService(
            IUnitOfWork unitOfWork,
            ILogger<CategoryService> logger,
            CategoryValidator validator,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<Result<IPagedList<CategoryViewModel>>> GetCategoriesAsync(
            int page = 1,
            int pageSize = 10,
            string? searchTerm = null,
            string? sortBy = null,
            bool ascending = true,
            CancellationToken cancellationToken = default)
        {
            try
            {
                page = Math.Max(1, page);
                pageSize = Math.Min(MaxPageSize, Math.Max(1, pageSize));

                Expression<Func<Category, bool>>? predicate = null;
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.Trim().ToLower();
                    predicate = c => c.Name.ToLower().Contains(searchTerm) || 
                                   (c.Description != null && c.Description.ToLower().Contains(searchTerm));
                }

                Func<IQueryable<Category>, IOrderedQueryable<Category>>? orderBy = null;
                if (!string.IsNullOrWhiteSpace(sortBy))
                {
                    orderBy = sortBy.ToLower() switch
                    {
                        "name" => query => ascending ? query.OrderBy(c => c.Name) : query.OrderByDescending(c => c.Name),
                        "createdat" => query => ascending ? query.OrderBy(c => c.CreatedAt) : query.OrderByDescending(c => c.CreatedAt),
                        "updatedat" => query => ascending ? query.OrderBy(c => c.UpdatedAt) : query.OrderByDescending(c => c.UpdatedAt),
                        _ => query => ascending ? query.OrderBy(c => c.Id) : query.OrderByDescending(c => c.Id)
                    };
                }

                var result = await _unitOfWork.Categories.GetPagedAsync(
                    page, 
                    pageSize, 
                    predicate, 
                    includeProperties: "Products",  // Include products to get accurate counts
                    cancellationToken: cancellationToken);
                    
                if (!result.IsSuccess)
                    return Result<IPagedList<CategoryViewModel>>.Failure(result.Message);

                // Use extension method to map paged list
                var pagedList = result.Value.ToMappedPagedList<Category, CategoryViewModel>(_mapper);
                
                return Result<IPagedList<CategoryViewModel>>.Success(pagedList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories");
                return Result<IPagedList<CategoryViewModel>>.Failure("Error retrieving categories");
            }
        }

        public async Task<Result<CategoryViewModel>> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken);
                if (!result.IsSuccess)
                    return Result<CategoryViewModel>.NotFound("Category");

                var viewModel = _mapper.Map<CategoryViewModel>(result.Value);
                return Result<CategoryViewModel>.Success(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category with ID {CategoryId}", id);
                return Result<CategoryViewModel>.Failure("Error retrieving category");
            }
        }

        public async Task<Result<CategoryViewModel>> CreateCategoryAsync(CategoryViewModel model, CancellationToken cancellationToken = default)
        {
            if (model == null)
                return Result<CategoryViewModel>.ValidationError("Category model cannot be null");

            try
            {
                var validationResult = await _validator.ValidateAsync(model, cancellationToken);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                    return Result<CategoryViewModel>.ValidationError("Validation failed", errors);
                }

                // Check if a soft-deleted category exists with the same name
                // This requires temporarily disabling the filter to find deleted categories
                var deletedCategory = await _unitOfWork.Context.Categories
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(c => c.Name == model.Name.Trim() && c.IsDeleted, cancellationToken);

                if (deletedCategory != null)
                {
                    // Option to restore the deleted category
                    deletedCategory.Restore(_unitOfWork.UserContext.CurrentUser);
                    deletedCategory.Description = model.Description?.Trim();
                    
                    var updateResult = await _unitOfWork.Categories.UpdateAsync(deletedCategory, cancellationToken);
                    if (!updateResult.IsSuccess)
                        return Result<CategoryViewModel>.Failure(updateResult.Message);
                    
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    
                    var restoredViewModel = _mapper.Map<CategoryViewModel>(updateResult.Value);
                    return Result<CategoryViewModel>.Success(restoredViewModel, 
                        "Category was restored from previously deleted state.");
                }

                // Create new category if no deleted category exists with the same name
                var category = _mapper.Map<Category>(model);
                category.IsActive = true;

                var result = await _unitOfWork.Categories.AddAsync(category, cancellationToken);
                if (!result.IsSuccess)
                    return Result<CategoryViewModel>.Failure(result.Message);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                var viewModel = _mapper.Map<CategoryViewModel>(result.Value);
                return Result<CategoryViewModel>.Success(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category: {CategoryName}", model.Name);
                return Result<CategoryViewModel>.Failure("Error creating category");
            }
        }

        public async Task<Result<CategoryViewModel>> UpdateCategoryAsync(int id, CategoryViewModel model, CancellationToken cancellationToken = default)
        {
            if (model == null)
                return Result<CategoryViewModel>.ValidationError("Category model cannot be null");

            if (id != model.Id)
                return Result<CategoryViewModel>.ValidationError("ID mismatch between URL and model");

            try
            {
                var validationResult = await _validator.ValidateAsync(model, cancellationToken);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                    return Result<CategoryViewModel>.ValidationError("Validation failed", errors);
                }

                var existingCategoryResult = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken);
                if (!existingCategoryResult.IsSuccess)
                    return Result<CategoryViewModel>.NotFound("Category");

                var existingCategory = existingCategoryResult.Value;
                _mapper.Map(model, existingCategory);

                var result = await _unitOfWork.Categories.UpdateAsync(existingCategory, cancellationToken);
                if (!result.IsSuccess)
                    return Result<CategoryViewModel>.Failure(result.Message);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                var viewModel = _mapper.Map<CategoryViewModel>(result.Value);
                return Result<CategoryViewModel>.Success(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category: {CategoryId}", id);
                return Result<CategoryViewModel>.Failure("Error updating category");
            }
        }

        public async Task<Result<bool>> DeleteCategoryAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _unitOfWork.Categories.DeleteAsync(id, cancellationToken);
                if (!result.IsSuccess)
                    return Result<bool>.Failure(result.Message);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category: {CategoryId}", id);
                return Result<bool>.Failure("Error deleting category");
            }
        }
    }
} 