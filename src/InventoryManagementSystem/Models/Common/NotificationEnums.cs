namespace InventoryManagementSystem.Models.Common
{
    public enum NotificationType
    {
        // System notifications
        System,
        Info,
        Success,
        Warning,
        Error,
        Alert,

        // Inventory-specific notifications
        LowStock,
        StockOut,
        OutOfStock,
        ExpiryWarning,
        OrderStatus,
        SystemAlert
    }

    public enum NotificationPriority
    {
        Low,
        Medium,
        High,
        Critical
    }
} 