namespace InventoryManagementSystem.Models.Common
{
    /// <summary>
    /// Enumeration for inventory transaction types
    /// </summary>
    public enum TransactionType
    {
        /// <summary>
        /// Increase in stock level (purchase, return, etc.)
        /// </summary>
        StockIn = 1,
        
        /// <summary>
        /// Decrease in stock level (sale, damage, etc.)
        /// </summary>
        StockOut = 2,
        
        /// <summary>
        /// Manual adjustment to match physical count
        /// </summary>
        Adjustment = 3,
        
        /// <summary>
        /// Initial stock setup
        /// </summary>
        Initial = 4,
        
        /// <summary>
        /// Transfer between locations (for future use)
        /// </summary>
        Transfer = 5
    }
} 