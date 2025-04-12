using InventoryManagementSystem.Models.Common;
using InventoryManagementSystem.Models.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace InventoryManagementSystem.Data.Repositories.Interfaces
{
    public interface IStockHistoryRepository : IRepository<StockHistory>
    {
        /// <summary>
        /// Retrieves a stock history record by ID with related Product and Category data
        /// </summary>
        Task<Result<StockHistory>> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
        
        Task<Result<IEnumerable<StockHistory>>> GetByProductIdAsync(int productId);
        Task<Result<IEnumerable<StockHistory>>> GetStockHistoryByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Result<IEnumerable<StockHistory>>> GetStockHistoryByTypeAsync(string type);
        Task<Result<int>> GetStockMovementCountAsync(int productId, TimeSpan period);
        Task<Result<decimal>> GetTotalStockInAsync(int productId);
        Task<Result<decimal>> GetTotalStockOutAsync(int productId);
    }
} 