using InventoryManagementSystem.Data.Context;
using InventoryManagementSystem.Data.Repositories.Interfaces;
using InventoryManagementSystem.Models.Common;
using InventoryManagementSystem.Models.Entities;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Data.Repositories
{
    public class StockHistoryRepository : Repository<StockHistory>, IStockHistoryRepository
    {
        private readonly new ILogger<StockHistoryRepository> _logger;

        public StockHistoryRepository(
            ApplicationDbContext context,
            ILogger<StockHistoryRepository> logger,
            IUserContextService userContext)
            : base(context, logger, userContext)
        {
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a stock history record by ID with related Product and Category data
        /// </summary>
        public async Task<Result<StockHistory>> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var stockHistory = await _dbSet
                    .Where(sh => sh.Id == id && !sh.IsDeleted)
                    .Include(sh => sh.Product)
                        .ThenInclude(p => p.Category)
                    .AsNoTracking() // For read-only operations
                    .FirstOrDefaultAsync(cancellationToken);

                if (stockHistory == null)
                    return Result<StockHistory>.NotFound("Stock history entry");

                return Result<StockHistory>.Success(stockHistory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving detailed stock history entry with ID {Id}", id);
                return Result<StockHistory>.Failure("Error retrieving stock history entry");
            }
        }

        public async Task<Result<IEnumerable<StockHistory>>> GetByProductIdAsync(int productId)
        {
            try
            {
                var stockHistory = await _dbSet
                    .Where(sh => sh.ProductId == productId && !sh.IsDeleted)
                    .OrderByDescending(sh => sh.Date)
                    .Include(sh => sh.Product)
                    .ToListAsync();

                return Result<IEnumerable<StockHistory>>.Success(stockHistory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stock history for product ID {ProductId}", productId);
                return Result<IEnumerable<StockHistory>>.Failure("Error retrieving stock history");
            }
        }

        public async Task<Result<IEnumerable<StockHistory>>> GetStockHistoryByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var stockHistory = await _dbSet
                    .Where(sh => sh.Date >= startDate && sh.Date <= endDate && !sh.IsDeleted)
                    .OrderByDescending(sh => sh.Date)
                    .Include(sh => sh.Product)
                    .ToListAsync();

                return Result<IEnumerable<StockHistory>>.Success(stockHistory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stock history for date range {StartDate} to {EndDate}", 
                    startDate, endDate);
                return Result<IEnumerable<StockHistory>>.Failure("Error retrieving stock history");
            }
        }

        public async Task<Result<IEnumerable<StockHistory>>> GetStockHistoryByTypeAsync(string typeString)
        {
            try
            {
                // Convert string type to enum
                TransactionType type = typeString.ToLower() switch
                {
                    "stock in" => TransactionType.StockIn,
                    "stock out" => TransactionType.StockOut,
                    "adjustment" => TransactionType.Adjustment,
                    "initial stock" => TransactionType.Initial,
                    "transfer" => TransactionType.Transfer,
                    _ => TransactionType.Adjustment
                };
                
                var stockHistory = await _dbSet
                    .Where(sh => sh.Type == type && !sh.IsDeleted)
                    .OrderByDescending(sh => sh.Date)
                    .Include(sh => sh.Product)
                    .ToListAsync();

                return Result<IEnumerable<StockHistory>>.Success(stockHistory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stock history of type {Type}", typeString);
                return Result<IEnumerable<StockHistory>>.Failure("Error retrieving stock history");
            }
        }

        public async Task<Result<int>> GetStockMovementCountAsync(int productId, TimeSpan period)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.Subtract(period);
                
                var count = await _dbSet
                    .CountAsync(sh => sh.ProductId == productId && sh.Date >= cutoffDate && !sh.IsDeleted);

                return Result<int>.Success(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stock movement count for product ID {ProductId}", productId);
                return Result<int>.Failure("Error retrieving stock movement count");
            }
        }

        public async Task<Result<decimal>> GetTotalStockInAsync(int productId)
        {
            try
            {
                var total = await _dbSet
                    .Where(sh => sh.ProductId == productId && sh.QuantityChange > 0 && !sh.IsDeleted)
                    .SumAsync(sh => (decimal)sh.QuantityChange);

                return Result<decimal>.Success(total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total stock in for product ID {ProductId}", productId);
                return Result<decimal>.Failure("Error calculating total stock in");
            }
        }

        public async Task<Result<decimal>> GetTotalStockOutAsync(int productId)
        {
            try
            {
                var total = await _dbSet
                    .Where(sh => sh.ProductId == productId && sh.QuantityChange < 0 && !sh.IsDeleted)
                    .SumAsync(sh => (decimal)Math.Abs(sh.QuantityChange));

                return Result<decimal>.Success(total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total stock out for product ID {ProductId}", productId);
                return Result<decimal>.Failure("Error calculating total stock out");
            }
        }
    }
} 