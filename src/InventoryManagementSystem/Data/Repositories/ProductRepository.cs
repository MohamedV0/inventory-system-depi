using InventoryManagementSystem.Data.Context;
using InventoryManagementSystem.Data.Repositories.Interfaces;
using InventoryManagementSystem.Data.Specifications;
using InventoryManagementSystem.Models.Common;
using InventoryManagementSystem.Models.Entities;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Data.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ILogger<ProductRepository> _productLogger;

        public ProductRepository(
            ApplicationDbContext context, 
            ILogger<ProductRepository> logger,
            IUserContextService userContext,
            ICacheService? cacheService = null) 
            : base(context, logger, userContext, cacheService)
        {
            _productLogger = logger;
        }

        public override async Task<Result<Product>> GetByIdAsync(int id, CancellationToken cancellationToken = default, bool trackChanges = false)
        {
            try
            {
                // Use the ProductDetailSpecification to get a product with all related entities
                var specification = new ProductDetailSpecification(id);
                
                // Override tracking if needed
                if (trackChanges)
                {
                    // You'd need to modify your specification to support this
                    // or handle it here
                }
                
                var result = await FirstOrDefaultAsync(specification, cancellationToken);
                return result;
            }
            catch (Exception ex)
            {
                _productLogger.LogError(ex, "Error retrieving product with ID {ProductId}", id);
                return Result<Product>.Failure($"Error retrieving product with ID {id}");
            }
        }

        public async Task<Result<bool>> ProductCodeExistsAsync(string code)
        {
            try
            {
                var exists = await _dbSet.AnyAsync(p => p.Code == code && !p.IsDeleted);
                return Result<bool>.Success(exists);
            }
            catch (Exception ex)
            {
                _productLogger.LogError(ex, "Error checking product code existence: {Code}", code);
                return Result<bool>.Failure("Error checking product code existence");
            }
        }

        public async Task<Result<bool>> ProductNameExistsAsync(string name)
        {
            try
            {
                var exists = await _dbSet.AnyAsync(p => p.Name == name && !p.IsDeleted);
                return Result<bool>.Success(exists);
            }
            catch (Exception ex)
            {
                _productLogger.LogError(ex, "Error checking product name existence: {Name}", name);
                return Result<bool>.Failure("Error checking product name existence");
            }
        }

        public async Task<Result<IEnumerable<Product>>> GetProductsByCategoryAsync(int categoryId)
        {
            try
            {
                var specification = new ProductsByCategorySpecification(categoryId);
                return await FindAsync(specification);
            }
            catch (Exception ex)
            {
                _productLogger.LogError(ex, "Error retrieving products for category: {CategoryId}", categoryId);
                return Result<IEnumerable<Product>>.Failure("Error retrieving products by category");
            }
        }

        public async Task<Result<IEnumerable<Product>>> GetLowStockProductsAsync(int threshold)
        {
            try
            {
                var specification = new LowStockProductsSpecification(threshold);
                return await FindAsync(specification);
            }
            catch (Exception ex)
            {
                _productLogger.LogError(ex, "Error retrieving low stock products with threshold {Threshold}", threshold);
                return Result<IEnumerable<Product>>.Failure("Error retrieving low stock products");
            }
        }

        public async Task<Result<Dictionary<string, decimal>>> GetProductStockLevelsAsync()
        {
            try
            {
                var products = await _dbSet
                    .Where(p => p.IsActive)
                    .Select(p => new { p.Name, p.CurrentStock })
                    .ToListAsync();

                var stockLevels = products.ToDictionary(p => p.Name, p => (decimal)p.CurrentStock);
                return Result<Dictionary<string, decimal>>.Success(stockLevels);
            }
            catch (Exception ex)
            {
                _productLogger.LogError(ex, "Error retrieving product stock levels");
                return Result<Dictionary<string, decimal>>.Failure("Error retrieving product stock levels");
            }
        }

        public async Task<Result<IEnumerable<Product>>> GetProductsCreatedAfterAsync(DateTime date)
        {
            try
            {
                var specification = new ProductsCreatedAfterSpecification(date);
                return await FindAsync(specification);
            }
            catch (Exception ex)
            {
                _productLogger.LogError(ex, "Error retrieving products created after {Date}", date);
                return Result<IEnumerable<Product>>.Failure("Error retrieving products by creation date");
            }
        }

        public async Task<Result<IEnumerable<Product>>> GetProductsUpdatedAfterAsync(DateTime date)
        {
            try
            {
                var specification = new ProductsUpdatedAfterSpecification(date);
                return await FindAsync(specification);
            }
            catch (Exception ex)
            {
                _productLogger.LogError(ex, "Error retrieving products updated after {Date}", date);
                return Result<IEnumerable<Product>>.Failure("Error retrieving products by update date");
            }
        }

        public async Task<Result<bool>> UpdateStockLevelAsync(int productId, int quantity)
        {
            try
            {
                var product = await _dbSet.FindAsync(productId);
                if (product == null || !product.IsActive)
                {
                    return Result<bool>.NotFound("Product not found");
                }

                product.CurrentStock = quantity;
                product.UpdateAuditFields(_userContext.CurrentUser);
                await _context.SaveChangesAsync();

                // Invalidate cache for this product
                InvalidateCacheKey($"{productId}");
                
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _productLogger.LogError(ex, "Error updating stock level for product {ProductId}", productId);
                return Result<bool>.Failure("Error updating product stock level");
            }
        }

        public async Task<Result<decimal>> GetTotalInventoryValueAsync()
        {
            try
            {
                var totalValue = await _dbSet
                    .Where(p => p.IsActive)
                    .SumAsync(p => p.CurrentStock * p.Price);

                return Result<decimal>.Success(totalValue);
            }
            catch (Exception ex)
            {
                _productLogger.LogError(ex, "Error calculating total inventory value");
                return Result<decimal>.Failure("Error calculating total inventory value");
            }
        }

        public async Task<Result<bool>> ProductExistsBySkuAsync(string sku)
        {
            try
            {
                var exists = await _dbSet.AnyAsync(p => p.SKU == sku && p.IsActive);
                return Result<bool>.Success(exists);
            }
            catch (Exception ex)
            {
                _productLogger.LogError(ex, "Error checking product SKU existence: {SKU}", sku);
                return Result<bool>.Failure("Error checking product SKU existence");
            }
        }

        public async Task<Result<IEnumerable<Product>>> GetProductsNeedingReorderAsync()
        {
            try
            {
                var specification = new ProductsNeedingReorderSpecification();
                return await FindAsync(specification);
            }
            catch (Exception ex)
            {
                _productLogger.LogError(ex, "Error retrieving products needing reorder");
                return Result<IEnumerable<Product>>.Failure("Error retrieving products needing reorder");
            }
        }
    }
} 