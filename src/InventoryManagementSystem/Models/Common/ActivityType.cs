namespace InventoryManagementSystem.Models.Common
{
    public enum ActivityType
    {
        // Product related activities
        ProductCreated,
        ProductUpdated,
        ProductDeleted,
        
        // Stock related activities
        StockAdded,
        StockRemoved,
        StockAdjusted,
        LowStockAlert,
        
        // Category related activities
        CategoryCreated,
        CategoryUpdated,
        CategoryDeleted,
        
        // Supplier related activities
        SupplierCreated,
        SupplierUpdated,
        SupplierDeleted,
        
        // User related activities
        UserLoggedIn,
        UserLoggedOut,
        UserProfileUpdated,
        
        // System activities
        SystemAlert,
        ConfigurationChanged
    }
} 