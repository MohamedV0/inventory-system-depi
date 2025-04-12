using System;
using System.Linq.Expressions;
using InventoryManagementSystem.Models.Entities;

namespace InventoryManagementSystem.Data.Specifications
{
    /// <summary>
    /// Base product specification with common includes and filters
    /// </summary>
    public class BaseProductSpecification : BaseSpecification<Product>
    {
        public BaseProductSpecification() 
            : base(p => !p.IsDeleted)
        {
            // Common includes and default ordering for products
            AddInclude(p => p.Category);
            ApplyOrderBy(p => p.Name);
        }
    }
    
    /// <summary>
    /// Gets a product by its ID with Category included
    /// </summary>
    public class ProductByIdSpecification : BaseProductSpecification
    {
        public ProductByIdSpecification(int id) 
            : base()
        {
            // Override the criteria
            Criteria = p => p.Id == id && !p.IsDeleted;
        }
    }
    
    /// <summary>
    /// Gets product details by ID with all related entities required for detail view
    /// </summary>
    public class ProductDetailSpecification : BaseProductSpecification
    {
        public ProductDetailSpecification(int id)
            : base()
        {
            Criteria = p => p.Id == id && !p.IsDeleted;
            
            // Include all required relationships for detailed view
            AddInclude(p => p.Category);
            AddInclude(p => p.ProductSuppliers);
            AddInclude("ProductSuppliers.Supplier");
            AddInclude(p => p.StockHistory);
        }
    }
    
    /// <summary>
    /// Gets a product by its SKU with Category included
    /// </summary>
    public class ProductBySkuSpecification : BaseProductSpecification
    {
        public ProductBySkuSpecification(string sku) 
            : base()
        {
            // Override the criteria
            Criteria = p => p.SKU == sku && !p.IsDeleted;
        }
    }
    
    /// <summary>
    /// Gets products by category ID
    /// </summary>
    public class ProductsByCategorySpecification : BaseProductSpecification
    {
        public ProductsByCategorySpecification(int categoryId) 
            : base()
        {
            Criteria = p => p.CategoryId == categoryId && !p.IsDeleted;
        }
    }
    
    /// <summary>
    /// Gets active products that need reordering (stock at or below reorder level)
    /// </summary>
    public class ProductsNeedingReorderSpecification : BaseProductSpecification
    {
        public ProductsNeedingReorderSpecification() 
            : base()
        {
            Criteria = p => p.IsActive && !p.IsDeleted && p.CurrentStock <= p.ReorderLevel;
            ApplyOrderBy(p => p.CurrentStock); // Order by lowest stock first
            
            // Include supplier relationships
            AddInclude(p => p.ProductSuppliers);
            AddInclude("ProductSuppliers.Supplier");
        }
    }
    
    /// <summary>
    /// Gets products with stock below a specified threshold
    /// </summary>
    public class LowStockProductsSpecification : BaseProductSpecification
    {
        public LowStockProductsSpecification(int threshold) 
            : base()
        {
            Criteria = p => p.IsActive && !p.IsDeleted && p.CurrentStock <= threshold;
            ApplyOrderBy(p => p.CurrentStock); // Order by lowest stock first
        }
    }
    
    /// <summary>
    /// Gets products with a specific search term in name or SKU
    /// </summary>
    public class ProductSearchSpecification : BaseProductSpecification
    {
        public ProductSearchSpecification(string searchTerm) 
            : base()
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return;
                
            var term = searchTerm.Trim().ToLower();
            Criteria = p => !p.IsDeleted && (
                p.Name.ToLower().Contains(term) || 
                p.Code.ToLower().Contains(term) || 
                p.SKU.ToLower().Contains(term)
            );
        }
    }
    
    /// <summary>
    /// Gets products created after a specified date
    /// </summary>
    public class ProductsCreatedAfterSpecification : BaseProductSpecification
    {
        public ProductsCreatedAfterSpecification(DateTime date) 
            : base()
        {
            Criteria = p => p.IsActive && !p.IsDeleted && p.CreatedAt > date;
            ApplyOrderByDescending(p => p.CreatedAt); // Most recently created first
        }
    }
    
    /// <summary>
    /// Gets products updated after a specified date
    /// </summary>
    public class ProductsUpdatedAfterSpecification : BaseProductSpecification
    {
        public ProductsUpdatedAfterSpecification(DateTime date) 
            : base()
        {
            Criteria = p => p.IsActive && !p.IsDeleted && p.UpdatedAt > date;
            ApplyOrderByDescending(p => p.UpdatedAt); // Most recently updated first
        }
    }
    
    /// <summary>
    /// Gets paginated products with optional filtering and sorting
    /// </summary>
    public class PaginatedProductsSpecification : BaseProductSpecification
    {
        public PaginatedProductsSpecification(
            int skip, 
            int take, 
            string? searchTerm = null, 
            int? categoryId = null,
            bool? lowStock = null,
            string? sortBy = null, 
            bool ascending = true)
            : base()
        {
            // Start with base criteria
            Expression<Func<Product, bool>> criteria = p => !p.IsDeleted;
            
            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                criteria = criteria.And(p => 
                    p.Name.ToLower().Contains(term) || 
                    p.Code.ToLower().Contains(term) || 
                    p.SKU.ToLower().Contains(term));
            }
            
            // Apply category filter if provided
            if (categoryId.HasValue)
            {
                criteria = criteria.And(p => p.CategoryId == categoryId.Value);
            }
            
            // Apply low stock filter if provided
            if (lowStock.HasValue && lowStock.Value)
            {
                criteria = criteria.And(p => p.CurrentStock <= p.ReorderLevel);
            }
            
            Criteria = criteria;
            
            // Apply sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "name":
                        if (ascending)
                            ApplyOrderBy(p => p.Name);
                        else
                            ApplyOrderByDescending(p => p.Name);
                        break;
                    case "price":
                        if (ascending)
                            ApplyOrderBy(p => p.Price);
                        else
                            ApplyOrderByDescending(p => p.Price);
                        break;
                    case "stock":
                        if (ascending)
                            ApplyOrderBy(p => p.CurrentStock);
                        else
                            ApplyOrderByDescending(p => p.CurrentStock);
                        break;
                    case "category":
                        if (ascending)
                            ApplyOrderBy(p => p.Category.Name);
                        else
                            ApplyOrderByDescending(p => p.Category.Name);
                        break;
                    default:
                        if (ascending)
                            ApplyOrderBy(p => p.Name);
                        else
                            ApplyOrderByDescending(p => p.Name);
                        break;
                }
            }
            else
            {
                // Default sort
                ApplyOrderBy(p => p.Name);
            }
            
            // Apply paging
            ApplyPaging(skip, take);
        }
    }
    
    /// <summary>
    /// Extension methods for combining expressions
    /// </summary>
    public static class ExpressionExtensions
    {
        public static Expression<Func<T, bool>> And<T>(
            this Expression<Func<T, bool>> left, 
            Expression<Func<T, bool>> right)
        {
            var param = Expression.Parameter(typeof(T), "x");
            var body = Expression.AndAlso(
                Expression.Invoke(left, param),
                Expression.Invoke(right, param)
            );
            return Expression.Lambda<Func<T, bool>>(body, param);
        }
    }
} 