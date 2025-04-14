using InventoryManagementSystem.Models.Identity;
using InventoryManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Services.Interfaces
{
    /// <summary>
    /// Service for managing users, roles, and permissions
    /// </summary>
    public interface IUserManagementService
    {
        /// <summary>
        /// Get all users
        /// </summary>
        Task<IEnumerable<UserViewModel>> GetUsersAsync();
        
        /// <summary>
        /// Get a user by ID
        /// </summary>
        Task<UserViewModel?> GetUserByIdAsync(string userId);
        
        /// <summary>
        /// Create a new user
        /// </summary>
        Task<IdentityResult> CreateUserAsync(CreateUserViewModel model);
        
        /// <summary>
        /// Update a user
        /// </summary>
        Task<IdentityResult> UpdateUserAsync(EditUserViewModel model);
        
        /// <summary>
        /// Delete a user
        /// </summary>
        Task<IdentityResult> DeleteUserAsync(string userId);
        
        /// <summary>
        /// Get all roles
        /// </summary>
        Task<IEnumerable<IdentityRole>> GetRolesAsync();
        
        /// <summary>
        /// Get all permissions
        /// </summary>
        Task<IEnumerable<Permission>> GetPermissionsAsync();
        
        /// <summary>
        /// Get permissions for a user
        /// </summary>
        Task<IEnumerable<UserPermissionViewModel>> GetUserPermissionsAsync(string userId);
        
        /// <summary>
        /// Get permissions for a user, grouped by category
        /// </summary>
        Task<Dictionary<string, List<UserPermissionViewModel>>> GetUserPermissionsByCategoryAsync(string userId);
        
        /// <summary>
        /// Get a user's permission by permission ID
        /// </summary>
        Task<UserPermission?> GetUserPermissionAsync(string userId, int permissionId);
        
        /// <summary>
        /// Update a user's permissions
        /// </summary>
        Task UpdateUserPermissionsAsync(string userId, IEnumerable<int> grantedPermissionIds);
        
        /// <summary>
        /// Create default permissions for the system
        /// </summary>
        Task CreateDefaultPermissionsAsync();
        
        /// <summary>
        /// Fix permissions for all admin users to ensure they have all permissions
        /// </summary>
        Task FixAdminPermissionsAsync();
    }
} 