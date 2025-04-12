using System.Collections.Generic;

namespace InventoryManagementSystem.Models.ViewModels
{
    /// <summary>
    /// View model for the dashboard
    /// </summary>
    public class DashboardViewModel
    {
        /// <summary>
        /// Total number of products in the system
        /// </summary>
        public int TotalProducts { get; set; }
        
        /// <summary>
        /// Total number of active suppliers
        /// </summary>
        public int TotalSuppliers { get; set; }
        
        /// <summary>
        /// Total number of categories
        /// </summary>
        public int TotalCategories { get; set; }
        
        /// <summary>
        /// Total value of inventory (cost * quantity)
        /// </summary>
        public decimal TotalInventoryValue { get; set; }
        
        /// <summary>
        /// Products that need to be reordered (stock level below reorder level)
        /// </summary>
        public List<ProductListItemViewModel> LowStockProducts { get; set; } = new List<ProductListItemViewModel>();
        
        /// <summary>
        /// Number of low stock products
        /// </summary>
        public int LowStockCount => LowStockProducts.Count;
        
        /// <summary>
        /// Whether there are any low stock products
        /// </summary>
        public bool HasLowStockProducts => LowStockCount > 0;
    }
} 