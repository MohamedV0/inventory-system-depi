using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementSystem.Data.Context;
using InventoryManagementSystem.Data.Repositories.Interfaces;
using InventoryManagementSystem.Models.Common;
using InventoryManagementSystem.Models.Entities;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Data.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly ILogger<CategoryRepository> _categoryLogger;
        
        public CategoryRepository(
            ApplicationDbContext context, 
            ILogger<CategoryRepository> logger,
            IUserContextService userContext,
            ICacheService? cacheService = null)
            : base(context, logger, userContext, cacheService)
        {
            _categoryLogger = logger;
        }

        public override async Task<Result<Category>> GetByIdAsync(int id, CancellationToken cancellationToken = default, bool trackChanges = false)
        {
            try
            {
                var query = _dbSet.Where(c => c.Id == id && c.IsActive);
                
                if (!trackChanges)
                {
                    query = query.AsNoTracking();
                }
                
                query = query.Include(c => c.Products.Where(p => p.IsActive));
                
                var category = await query.FirstOrDefaultAsync(cancellationToken);

                if (category == null)
                {
                    return Result<Category>.NotFound($"Category with ID {id} not found");
                }

                return Result<Category>.Success(category);
            }
            catch (Exception ex)
            {
                _categoryLogger.LogError(ex, "Error retrieving category {CategoryId}", id);
                return Result<Category>.Failure("Error retrieving category");
            }
        }

        public override async Task<Result<IEnumerable<Category>>> GetAllAsync(
            CancellationToken cancellationToken = default,
            QueryOptions? options = null)
        {
            try
            {
                var query = _dbSet.Where(c => c.IsActive);
                
                if (options == null || !options.TrackingEnabled)
                {
                    query = query.AsNoTracking();
                }
                
                var categories = await query
                    .OrderBy(c => c.Name)
                    .ToListAsync(cancellationToken);

                return Result<IEnumerable<Category>>.Success(categories);
            }
            catch (Exception ex)
            {
                _categoryLogger.LogError(ex, "Error retrieving all categories");
                return Result<IEnumerable<Category>>.Failure("Error retrieving categories");
            }
        }

        public async Task<Result<bool>> CategoryNameExistsAsync(string name, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return Result<bool>.ValidationError("Category name cannot be empty");
                }

                bool exists = await _dbSet
                    .Where(c => c.IsActive)
                    .AnyAsync(c => c.Name.ToLower() == name.ToLower().Trim(), cancellationToken);

                return Result<bool>.Success(exists);
            }
            catch (Exception ex)
            {
                _categoryLogger.LogError(ex, "Error checking if category name {CategoryName} exists", name);
                return Result<bool>.Failure("Error checking if category name exists");
            }
        }

        public async Task<Result<bool>> HasActiveProductsAsync(int categoryId, CancellationToken cancellationToken = default)
        {
            try
            {
                bool hasActiveProducts = await _context.Products
                    .Where(p => p.IsActive && p.CategoryId == categoryId)
                    .AnyAsync(cancellationToken);

                return Result<bool>.Success(hasActiveProducts);
            }
            catch (Exception ex)
            {
                _categoryLogger.LogError(ex, "Error checking if category {CategoryId} has active products", categoryId);
                return Result<bool>.Failure("Error checking if category has active products");
            }
        }

        public async Task<Result<Dictionary<string, int>>> GetCategoryProductCountsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var productCounts = await _dbSet
                    .Where(c => c.IsActive)
                    .Select(c => new
                    {
                        CategoryName = c.Name,
                        ProductCount = c.Products.Count(p => p.IsActive)
                    })
                    .ToDictionaryAsync(x => x.CategoryName, x => x.ProductCount, cancellationToken);

                return Result<Dictionary<string, int>>.Success(productCounts);
            }
            catch (Exception ex)
            {
                _categoryLogger.LogError(ex, "Error retrieving category product counts");
                return Result<Dictionary<string, int>>.Failure("Error retrieving category product counts");
            }
        }

        public async Task<Result<IEnumerable<Category>>> GetCategoriesCreatedAfterAsync(DateTime date, CancellationToken cancellationToken = default)
        {
            try
            {
                var categories = await _dbSet
                    .Where(c => c.IsActive && c.CreatedAt > date)
                    .ToListAsync(cancellationToken);

                return Result<IEnumerable<Category>>.Success(categories);
            }
            catch (Exception ex)
            {
                _categoryLogger.LogError(ex, "Error retrieving categories created after {Date}", date);
                return Result<IEnumerable<Category>>.Failure("Error retrieving categories by creation date");
            }
        }

        public async Task<Result<IEnumerable<Category>>> GetCategoriesUpdatedAfterAsync(DateTime date, CancellationToken cancellationToken = default)
        {
            try
            {
                var categories = await _dbSet
                    .Where(c => c.IsActive && c.UpdatedAt > date)
                    .ToListAsync(cancellationToken);

                return Result<IEnumerable<Category>>.Success(categories);
            }
            catch (Exception ex)
            {
                _categoryLogger.LogError(ex, "Error retrieving categories updated after {Date}", date);
                return Result<IEnumerable<Category>>.Failure("Error retrieving categories by update date");
            }
        }

        public async Task<Result<IEnumerable<Category>>> GetCategoriesWithProductsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var categories = await _dbSet
                    .Where(c => c.IsActive && c.Products.Any(p => p.IsActive))
                    .Include(c => c.Products.Where(p => p.IsActive))
                    .ToListAsync(cancellationToken);

                return Result<IEnumerable<Category>>.Success(categories);
            }
            catch (Exception ex)
            {
                _categoryLogger.LogError(ex, "Error retrieving categories with products");
                return Result<IEnumerable<Category>>.Failure("Error retrieving categories with products");
            }
        }

        public async Task<Result<IEnumerable<Category>>> GetEmptyCategoriesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var categories = await _dbSet
                    .Where(c => c.IsActive && !c.Products.Any(p => p.IsActive))
                    .ToListAsync(cancellationToken);

                return Result<IEnumerable<Category>>.Success(categories);
            }
            catch (Exception ex)
            {
                _categoryLogger.LogError(ex, "Error retrieving empty categories");
                return Result<IEnumerable<Category>>.Failure("Error retrieving empty categories");
            }
        }

        public override async Task<Result<Category>> AddAsync(Category entity, CancellationToken cancellationToken = default)
        {
            try
            {
                // Check if category name already exists
                var nameExistsResult = await CategoryNameExistsAsync(entity.Name, cancellationToken);
                if (!nameExistsResult.IsSuccess)
                {
                    return Result<Category>.Failure(nameExistsResult.Message);
                }

                if (nameExistsResult.Value)
                {
                    return Result<Category>.Failure($"A category with the name '{entity.Name}' already exists");
                }

                await _dbSet.AddAsync(entity, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                return Result<Category>.Success(entity);
            }
            catch (Exception ex)
            {
                _categoryLogger.LogError(ex, "Error adding category {CategoryName}", entity.Name);
                return Result<Category>.Failure("Error adding category");
            }
        }

        public override async Task<Result<Category>> UpdateAsync(Category entity, CancellationToken cancellationToken = default)
        {
            try
            {
                var existingCategory = await _dbSet.FindAsync(new object[] { entity.Id }, cancellationToken);
                if (existingCategory == null)
                {
                    return Result<Category>.NotFound($"Category with ID {entity.Id} not found");
                }

                // Check if another category with the same name exists (excluding current category)
                var duplicateExists = await _dbSet
                    .Where(c => c.IsActive && c.Id != entity.Id)
                    .AnyAsync(c => c.Name.ToLower() == entity.Name.ToLower(), cancellationToken);

                if (duplicateExists)
                {
                    return Result<Category>.Failure($"Another category with the name '{entity.Name}' already exists");
                }

                _context.Entry(existingCategory).CurrentValues.SetValues(entity);
                entity.UpdateAuditFields(_userContext.CurrentUser);
                await _context.SaveChangesAsync(cancellationToken);
                return Result<Category>.Success(entity);
            }
            catch (Exception ex)
            {
                _categoryLogger.LogError(ex, "Error updating category {CategoryId}", entity.Id);
                return Result<Category>.Failure("Error updating category");
            }
        }

        public override async Task<Result<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var category = await _dbSet.FindAsync(new object[] { id }, cancellationToken);
                if (category == null)
                {
                    return Result<bool>.NotFound($"Category with ID {id} not found");
                }

                // Check if category has active products
                var hasProducts = await _context.Products
                    .Where(p => p.IsActive && p.CategoryId == id)
                    .AnyAsync(cancellationToken);

                if (hasProducts)
                {
                    return Result<bool>.Failure(
                        "Cannot delete category that has active products. Please remove or reassign the products first.");
                }

                // Perform soft delete
                category.Delete(_userContext.CurrentUser);
                await _context.SaveChangesAsync(cancellationToken);

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _categoryLogger.LogError(ex, "Error deleting category {CategoryId}", id);
                return Result<bool>.Failure("Error deleting category");
            }
        }
    }
} 