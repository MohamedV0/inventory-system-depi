namespace InventoryManagementSystem.Services.Interfaces
{
    /// <summary>
    /// Service interface for managing user context
    /// </summary>
    public interface IUserContextService
    {
        string CurrentUser { get; }
        string GetCurrentUserId();
        
        /// <summary>
        /// Gets the current user ID asynchronously
        /// </summary>
        /// <returns>The current user ID or a default value if no user is authenticated</returns>
        Task<string> GetCurrentUserIdAsync();
        
        /// <summary>
        /// Gets the current username asynchronously
        /// </summary>
        /// <returns>The current username or a default value if no user is authenticated</returns>
        Task<string> GetCurrentUserNameAsync();
    }
} 