using Microsoft.AspNetCore.Authorization;

namespace InventoryManagementSystem.Extensions
{
    /// <summary>
    /// Extension methods for AuthorizationPolicyBuilder
    /// </summary>
    public static class AuthorizationPolicyBuilderExtensions
    {
        /// <summary>
        /// Adds a policy to require a specific permission
        /// </summary>
        /// <param name="builder">The authorization policy builder</param>
        /// <param name="permissionName">The name of the permission to require</param>
        /// <returns>The authorization policy builder</returns>
        public static AuthorizationPolicyBuilder RequirePermission(
            this AuthorizationPolicyBuilder builder, 
            string permissionName)
        {
            return builder.AddRequirements(new PermissionRequirement(permissionName));
        }
    }
} 