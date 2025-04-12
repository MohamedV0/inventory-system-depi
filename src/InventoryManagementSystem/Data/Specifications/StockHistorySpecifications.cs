using System;
using System.Linq.Expressions;
using InventoryManagementSystem.Models.Entities;
using InventoryManagementSystem.Models.Common;

namespace InventoryManagementSystem.Data.Specifications
{
    /// <summary>
    /// Base stock history specification with common includes and filters
    /// </summary>
    public class BaseStockHistorySpecification : BaseSpecification<StockHistory>
    {
        public BaseStockHistorySpecification() 
            : base(sh => !sh.IsDeleted)
        {
            // Common includes and default ordering for stock history
            AddInclude(sh => sh.Product);
            ApplyOrderByDescending(sh => sh.Date);
        }
    }
    
    /// <summary>
    /// Gets stock history for a specific product
    /// </summary>
    public class StockHistoryByProductSpecification : BaseStockHistorySpecification
    {
        public StockHistoryByProductSpecification(int productId) 
            : base()
        {
            // Override the criteria
            Criteria = sh => sh.ProductId == productId && !sh.IsDeleted;
        }
    }
    
    /// <summary>
    /// Gets stock history created within a date range
    /// </summary>
    public class StockHistoryByDateRangeSpecification : BaseStockHistorySpecification
    {
        public StockHistoryByDateRangeSpecification(DateTime startDate, DateTime endDate) 
            : base()
        {
            Criteria = sh => !sh.IsDeleted && sh.Date >= startDate && sh.Date <= endDate;
        }
    }
    
    /// <summary>
    /// Gets stock history by transaction type
    /// </summary>
    public class StockHistoryByTypeSpecification : BaseStockHistorySpecification
    {
        public StockHistoryByTypeSpecification(TransactionType type) 
            : base()
        {
            Criteria = sh => sh.Type == type && !sh.IsDeleted;
        }
    }
    
    /// <summary>
    /// Gets paginated stock history with optional filtering
    /// </summary>
    public class PaginatedStockHistorySpecification : BaseStockHistorySpecification
    {
        public PaginatedStockHistorySpecification(
            int skip, 
            int take, 
            int? productId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            TransactionType? type = null)
            : base()
        {
            // Start with base criteria
            Expression<Func<StockHistory, bool>> criteria = sh => !sh.IsDeleted;
            
            // Apply product filter if provided
            if (productId.HasValue)
            {
                criteria = criteria.And(sh => sh.ProductId == productId.Value);
            }
            
            // Apply date range filter if provided
            if (startDate.HasValue)
            {
                criteria = criteria.And(sh => sh.Date >= startDate.Value);
            }
            
            if (endDate.HasValue)
            {
                criteria = criteria.And(sh => sh.Date <= endDate.Value);
            }
            
            // Apply transaction type filter if provided
            if (type.HasValue)
            {
                criteria = criteria.And(sh => sh.Type == type.Value);
            }
            
            Criteria = criteria;
            
            // Apply paging
            ApplyPaging(skip, take);
        }
    }
} 