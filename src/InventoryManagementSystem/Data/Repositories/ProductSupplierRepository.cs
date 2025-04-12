using InventoryManagementSystem.Data.Context;
using InventoryManagementSystem.Data.Repositories.Interfaces;
using InventoryManagementSystem.Models.Common;
using InventoryManagementSystem.Models.Entities;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace InventoryManagementSystem.Data.Repositories
{
    public class ProductSupplierRepository : Repository<ProductSupplier>, IProductSupplierRepository
    {
        private new readonly ApplicationDbContext _context;
        private new readonly DbSet<ProductSupplier> _dbSet;
        private new readonly IUserContextService _userContext;
        private new readonly ILogger<ProductSupplierRepository> _logger;

        public ProductSupplierRepository(
            ApplicationDbContext context,
            ILogger<ProductSupplierRepository> logger,
            IUserContextService userContext)
            : base(context, logger, userContext)
        {
            _context = context;
            _dbSet = context.Set<ProductSupplier>();
            _userContext = userContext;
            _logger = logger;
        }

        public async Task<Result<bool>> ExistsAsync(int productId, int supplierId)
        {
            try
            {
                var exists = await _dbSet.AnyAsync(ps => 
                    ps.ProductId == productId && 
                    ps.SupplierId == supplierId && 
                    !ps.IsDeleted);
                
                return Result<bool>.Success(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if product-supplier relationship exists: ProductId={ProductId}, SupplierId={SupplierId}", 
                    productId, supplierId);
                return Result<bool>.Failure("Error checking product-supplier relationship existence");
            }
        }

        public async Task<Result<ProductSupplier>> GetByCompositeIdAsync(int productId, int supplierId, string? includeProperties = null)
        {
            try
            {
                var query = _dbSet.AsQueryable();

                if (!string.IsNullOrWhiteSpace(includeProperties))
                {
                    foreach (var includeProperty in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    {
                        query = query.Include(includeProperty.Trim());
                    }
                }

                var productSupplier = await query.FirstOrDefaultAsync(ps => 
                    ps.ProductId == productId && 
                    ps.SupplierId == supplierId && 
                    !ps.IsDeleted);

                if (productSupplier == null)
                {
                    return Result<ProductSupplier>.NotFound($"ProductSupplier with ProductId={productId} and SupplierId={supplierId}");
                }

                return Result<ProductSupplier>.Success(productSupplier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product-supplier relationship: ProductId={ProductId}, SupplierId={SupplierId}", 
                    productId, supplierId);
                return Result<ProductSupplier>.Failure("Error retrieving product-supplier relationship");
            }
        }

        public async Task<Result<bool>> DeleteAsync(int productId, int supplierId)
        {
            try
            {
                var productSupplier = await _dbSet.FirstOrDefaultAsync(ps => 
                    ps.ProductId == productId && 
                    ps.SupplierId == supplierId && 
                    !ps.IsDeleted);

                if (productSupplier == null)
                {
                    return Result<bool>.NotFound($"ProductSupplier with ProductId={productId} and SupplierId={supplierId}");
                }

                productSupplier.Delete(_userContext.CurrentUser);
                await _context.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product-supplier relationship: ProductId={ProductId}, SupplierId={SupplierId}", 
                    productId, supplierId);
                return Result<bool>.Failure("Error deleting product-supplier relationship");
            }
        }

        public async Task<Result<ProductSupplier>> GetPreferredSupplierForProductAsync(int productId)
        {
            try
            {
                var preferredSupplier = await _dbSet
                    .Include(ps => ps.Supplier)
                    .FirstOrDefaultAsync(ps => ps.ProductId == productId && ps.IsPreferred);
                
                if (preferredSupplier == null)
                    return Result<ProductSupplier>.NotFound("Preferred supplier not found for product");
                
                return Result<ProductSupplier>.Success(preferredSupplier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving preferred supplier for product {ProductId}", productId);
                return Result<ProductSupplier>.Failure("Error retrieving preferred supplier");
            }
        }

        public async Task<Result<bool>> UnsetPreferredSupplierAsync(int productId, CancellationToken cancellationToken = default)
        {
            try
            {
                var suppliers = await _dbSet
                    .Where(ps => ps.ProductId == productId && ps.IsPreferred)
                    .ToListAsync(cancellationToken);
                
                if (suppliers.Count == 0)
                    return Result<bool>.Success(true);
                
                foreach (var supplier in suppliers)
                {
                    supplier.IsPreferred = false;
                    supplier.UpdateAuditFields(_userContext.CurrentUser);
                }
                
                await _context.SaveChangesAsync(cancellationToken);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unsetting preferred suppliers for product {ProductId}", productId);
                return Result<bool>.Failure("Error unsetting preferred suppliers");
            }
        }

        public async Task<Result<bool>> SetPreferredSupplierAsync(int productId, int supplierId, CancellationToken cancellationToken = default)
        {
            try
            {
                // First unset any existing preferred suppliers
                var unsetResult = await UnsetPreferredSupplierAsync(productId, cancellationToken);
                if (!unsetResult.IsSuccess)
                    return Result<bool>.Failure(unsetResult.Message);
                
                // Now set the new preferred supplier
                var supplier = await _dbSet.FirstOrDefaultAsync(ps => 
                    ps.ProductId == productId && ps.SupplierId == supplierId, cancellationToken);
                
                if (supplier == null)
                    return Result<bool>.NotFound("Product-supplier relationship not found");
                
                supplier.IsPreferred = true;
                supplier.UpdateAuditFields(_userContext.CurrentUser);
                
                await _context.SaveChangesAsync(cancellationToken);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting preferred supplier {SupplierId} for product {ProductId}", 
                    supplierId, productId);
                return Result<bool>.Failure("Error setting preferred supplier");
            }
        }

        public async Task<Result<bool>> UpdateLastPurchaseDateAsync(int productId, int supplierId, DateTime purchaseDate, CancellationToken cancellationToken = default)
        {
            try
            {
                var productSupplier = await _dbSet.FirstOrDefaultAsync(ps => 
                    ps.ProductId == productId && ps.SupplierId == supplierId, cancellationToken);
                
                if (productSupplier == null)
                    return Result<bool>.NotFound("Product-supplier relationship not found");
                
                productSupplier.LastPurchaseDate = purchaseDate;
                productSupplier.UpdateAuditFields(_userContext.CurrentUser);
                
                await _context.SaveChangesAsync(cancellationToken);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating last purchase date for product {ProductId} and supplier {SupplierId}", 
                    productId, supplierId);
                return Result<bool>.Failure("Error updating last purchase date");
            }
        }

        public async Task<Result<bool>> RelationshipExistsAsync(int productId, int supplierId)
        {
            try
            {
                var exists = await _dbSet.AnyAsync(ps => 
                    ps.ProductId == productId && 
                    ps.SupplierId == supplierId && 
                    !ps.IsDeleted);
                
                return Result<bool>.Success(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if product-supplier relationship exists for Product {ProductId} and Supplier {SupplierId}", 
                    productId, supplierId);
                return Result<bool>.Failure("Error checking product-supplier relationship existence");
            }
        }
    }
} 