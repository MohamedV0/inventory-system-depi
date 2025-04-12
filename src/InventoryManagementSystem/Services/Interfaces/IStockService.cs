using InventoryManagementSystem.Models.Common;
using InventoryManagementSystem.Models.ViewModels;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using X.PagedList;

namespace InventoryManagementSystem.Services.Interfaces
{
    /// <summary>
    /// Interface for stock movement and transaction operations
    /// </summary>
    public interface IStockService
    {
        /// <summary>
        /// Gets a paginated list of stock history entries
        /// </summary>
        Task<Result<IPagedList<StockHistoryViewModel>>> GetStockHistoryAsync(
            int page = 1,
            int pageSize = 10,
            string? searchTerm = null,
            string? referenceSearch = null,
            string? reasonSearch = null,
            string? sortBy = null,
            bool ascending = true,
            int? productId = null,
            int? transactionType = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a specific stock history entry by ID
        /// </summary>
        Task<Result<StockHistoryDetailViewModel>> GetStockHistoryByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Records a stock addition transaction
        /// </summary>
        Task<Result<StockHistoryViewModel>> AddStockAsync(AddStockViewModel model, CancellationToken cancellationToken = default);

        /// <summary>
        /// Records a stock removal transaction
        /// </summary>
        Task<Result<StockHistoryViewModel>> RemoveStockAsync(RemoveStockViewModel model, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets stock history for a specific product
        /// </summary>
        Task<Result<ProductStockHistoryViewModel>> GetProductStockHistoryAsync(int productId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets products that need reordering (stock level below reorder point)
        /// </summary>
        Task<Result<IEnumerable<ProductListItemViewModel>>> GetProductsNeedingReorderAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets low stock items formatted for the low stock view
        /// </summary>
        Task<Result<IEnumerable<LowStockViewModel>>> GetLowStockItemsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Adjusts a product's stock level to match a physical count
        /// </summary>
        Task<Result<StockHistoryViewModel>> AdjustStockAsync(AdjustStockViewModel model, CancellationToken cancellationToken = default);
    }
} 