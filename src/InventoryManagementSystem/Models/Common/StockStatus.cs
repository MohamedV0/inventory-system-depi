namespace InventoryManagementSystem.Models.Common
{
    /// <summary>
    /// Enumeration for stock level status
    /// </summary>
    public enum StockStatus
    {
        /// <summary>
        /// Stock level is below reorder level
        /// </summary>
        Low = 1,
        
        /// <summary>
        /// Stock level is above reorder level but below maximum
        /// </summary>
        Normal = 2,
        
        /// <summary>
        /// Stock level is at or above maximum level
        /// </summary>
        High = 3
    }
} 