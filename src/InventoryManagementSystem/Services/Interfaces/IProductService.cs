using InventoryManagementSystem.Models.Common;
using InventoryManagementSystem.Models.ViewModels;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using X.PagedList;

namespace InventoryManagementSystem.Services.Interfaces
{
    /// <summary>
    /// Interface for product management operations
    /// </summary>
    public interface IProductService
    {
        /// <summary>
        /// Gets a paginated list of products
        /// </summary>
        Task<Result<IEnumerable<ProductListItemViewModel>>> GetProductsAsync(
            int page = 1, 
            int pageSize = 10,
            string? searchTerm = null,
            string? sortBy = null,
            bool ascending = true,
            int? categoryId = null,
            bool? lowStock = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a product by its ID
        /// </summary>
        Task<Result<ProductDetailsViewModel>> GetProductByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new product
        /// </summary>
        Task<Result<ProductDetailsViewModel>> CreateProductAsync(CreateProductViewModel model, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing product
        /// </summary>
        Task<Result<ProductDetailsViewModel>> UpdateProductAsync(int id, UpdateProductViewModel model, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a product
        /// </summary>
        Task<Result<bool>> DeleteProductAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the stock level of a product
        /// </summary>
        Task<Result<ProductDetailsViewModel>> UpdateStockLevelAsync(int id, int quantity, string reason, CancellationToken cancellationToken = default);

        Task<Result<IEnumerable<ProductListItemViewModel>>> GetProductsNeedingReorderAsync(CancellationToken cancellationToken = default);
        Task<Result<IEnumerable<StockHistoryViewModel>>> GetStockHistoryAsync(int productId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets a paged list of products
        /// </summary>
        /// <param name="page">The page number (1-based)</param>
        /// <param name="pageSize">The number of items per page</param>
        /// <param name="searchTerm">Optional search term to filter products by name, SKU, or description</param>
        /// <param name="categoryName">Optional category name to filter products by category</param>
        /// <param name="lowStock">Optional filter to show only products with low stock (stock <= reorder level)</param>
        /// <param name="outOfStock">Optional filter to show only products that are out of stock (stock = 0)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A paged list of products</returns>
        Task<Result<IPagedList<ProductListItemViewModel>>> GetPagedProductsAsync(
            int page = 1, 
            int pageSize = 10,
            string searchTerm = null,
            string categoryName = null,
            bool? lowStock = null,
            bool? outOfStock = null,
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Checks if a product exists by SKU
        /// </summary>
        Task<Result<bool>> ProductExistsBySkuAsync(string sku, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets products by category ID
        /// </summary>
        Task<Result<IEnumerable<ProductListItemViewModel>>> GetProductsByCategoryAsync(int categoryId, CancellationToken cancellationToken = default);
    }
} 