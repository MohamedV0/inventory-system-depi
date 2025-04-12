using InventoryManagementSystem.Models.Common;
using InventoryManagementSystem.Models.ViewModels;

namespace InventoryManagementSystem.Services.Interfaces
{
    /// <summary>
    /// Interface for product-supplier relationship management operations
    /// </summary>
    public interface IProductSupplierService
    {
        /// <summary>
        /// Gets a paginated list of product-supplier relationships
        /// </summary>
        Task<Result<IEnumerable<ProductSupplierListItemViewModel>>> GetProductSuppliersAsync(
            int page = 1,
            int pageSize = 10,
            string? sortBy = null,
            bool ascending = true,
            int? productId = null,
            int? supplierId = null);

        /// <summary>
        /// Gets a product-supplier relationship by product and supplier IDs
        /// </summary>
        Task<Result<ProductSupplierDetailsViewModel>> GetProductSupplierAsync(int productId, int supplierId);

        /// <summary>
        /// Creates a new product-supplier relationship
        /// </summary>
        Task<Result<ProductSupplierDetailsViewModel>> CreateProductSupplierAsync(CreateProductSupplierViewModel model);

        /// <summary>
        /// Updates an existing product-supplier relationship
        /// </summary>
        Task<Result<ProductSupplierDetailsViewModel>> UpdateProductSupplierAsync(
            int productId,
            int supplierId,
            UpdateProductSupplierViewModel model);

        /// <summary>
        /// Deletes a product-supplier relationship
        /// </summary>
        Task<Result<bool>> DeleteProductSupplierAsync(int productId, int supplierId);
        
        /// <summary>
        /// Gets the preferred supplier for a product
        /// </summary>
        Task<Result<SupplierDetailsViewModel>> GetPreferredSupplierForProductAsync(
            int productId,
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Sets the preferred supplier for a product
        /// </summary>
        Task<Result<bool>> SetPreferredSupplierAsync(
            int productId,
            int supplierId,
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Updates the last purchase date for a product-supplier relationship
        /// </summary>
        Task<Result<bool>> UpdateLastPurchaseDateAsync(
            int productId,
            int supplierId,
            DateTime purchaseDate,
            CancellationToken cancellationToken = default);
    }
} 