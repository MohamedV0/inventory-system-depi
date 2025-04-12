namespace InventoryManagementSystem.Services.Interfaces
{
    /// <summary>
    /// Service interface for managing user context
    /// </summary>
    public interface IUserContextService
    {
        string CurrentUser { get; }
        string GetCurrentUserId();
    }
} 